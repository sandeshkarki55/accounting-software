using System.Diagnostics;
using System.Text;

namespace AccountingApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private const int MaxBodyBytes = 1024 * 1024; // 1 MB limit
    private static readonly HashSet<string> BinaryContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/octet-stream",
        "image/", "video/", "audio/",
        "application/pdf",
        "application/zip",
        "application/x-gzip"
    };

    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for GET requests and binary content types
        if (HttpMethods.IsGet(context.Request.Method) || IsBinaryContent(context.Request.ContentType))
        {
            await _next(context);
            return;
        }

        var activity = Activity.Current;
        if (activity == null)
        {
            await _next(context);
            return;
        }

        // Add request body to span (with size limit)
        context.Request.EnableBuffering();
        var requestBody = await ReadStreamToStringAsync(context.Request.Body, MaxBodyBytes);
        activity.SetTag("http.request.body", requestBody);
        context.Request.Body.Position = 0;

        // Capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Add response body to span (with size limit)
        if (!IsBinaryContent(context.Response.ContentType))
        {
            var responseBodyContent = await ReadStreamToStringAsync(context.Response.Body, MaxBodyBytes);
            activity.SetTag("http.response.body", responseBodyContent);
        }
        responseBody.Position = 0;
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static bool IsBinaryContent(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        return BinaryContentTypes.Any(prefix => contentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string> ReadStreamToStringAsync(Stream stream, int maxBytes)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var buffer = new char[maxBytes];
        var charsRead = await reader.ReadAsync(buffer, 0, maxBytes);
        stream.Position = 0;
        var result = new string(buffer, 0, charsRead);
        return charsRead >= maxBytes ? result + "... [truncated]" : result;
    }
}