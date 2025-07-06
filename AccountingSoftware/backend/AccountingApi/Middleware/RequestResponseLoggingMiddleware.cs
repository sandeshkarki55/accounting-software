using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AccountingApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var activity = Activity.Current;

        // Add request body to span
        context.Request.EnableBuffering();
        var requestBody = await ReadStreamToStringAsync(context.Request.Body);
        activity?.SetTag("http.request.body", requestBody);
        context.Request.Body.Position = 0;

        // Capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Add response body to span
        var responseBodyContent = await ReadStreamToStringAsync(context.Response.Body);
        activity?.SetTag("http.response.body", responseBodyContent);
        responseBody.Position = 0;
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static async Task<string> ReadStreamToStringAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var result = await reader.ReadToEndAsync();
        stream.Position = 0;
        return result;
    }
}
