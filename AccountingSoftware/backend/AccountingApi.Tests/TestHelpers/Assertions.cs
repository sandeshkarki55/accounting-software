using AccountingApi.DTOs.Authentication;

using Microsoft.AspNetCore.Mvc;

namespace AccountingApi.Tests.TestHelpers;

public static class ResultAssertions
{
    public static ApiResponseDto<T> Ok<T>(ActionResult<ApiResponseDto<T>> result)
    {
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null, "Expected OkObjectResult");
        var payload = ok!.Value as ApiResponseDto<T>;
        Assert.That(payload, Is.Not.Null, "Expected ApiResponseDto payload");
        return payload!;
    }

    public static ApiResponseDto<T> BadRequest<T>(ActionResult<ApiResponseDto<T>> result)
    {
        var bad = result.Result as BadRequestObjectResult;
        Assert.That(bad, Is.Not.Null, "Expected BadRequestObjectResult");
        var payload = bad!.Value as ApiResponseDto<T>;
        Assert.That(payload, Is.Not.Null, "Expected ApiResponseDto payload");
        return payload!;
    }

    public static ApiResponseDto<T> Created<T>(ActionResult<ApiResponseDto<T>> result)
    {
        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null, "Expected CreatedAtActionResult");
        var payload = created!.Value as ApiResponseDto<T>;
        Assert.That(payload, Is.Not.Null, "Expected ApiResponseDto payload");
        return payload!;
    }

    public static ApiResponseDto<T> Unauthorized<T>(ActionResult<ApiResponseDto<T>> result)
    {
        var unauthorized = result.Result as UnauthorizedObjectResult;
        Assert.That(unauthorized, Is.Not.Null, "Expected UnauthorizedObjectResult");
        var payload = unauthorized!.Value as ApiResponseDto<T>;
        Assert.That(payload, Is.Not.Null, "Expected ApiResponseDto payload");
        return payload!;
    }
}

public static class ApiResponseAssertions
{
    public static T EnsureSuccess<T>(ApiResponseDto<T> dto)
    {
        Assert.That(dto.Success, Is.True, dto.Message);
        Assert.That(dto.Data, Is.Not.Null);
        return dto.Data!;
    }

    public static void EnsureFailure<T>(ApiResponseDto<T> dto)
    {
        Assert.That(dto.Success, Is.False, "Expected failure response");
        Assert.That(dto.Errors, Is.Not.Null);
        Assert.That(dto.Errors.Count, Is.GreaterThan(0));
    }

    public static void HasError<T>(ApiResponseDto<T> dto, string contains)
    {
        Assert.That(dto.Errors.Any(e => e.Contains(contains, StringComparison.OrdinalIgnoreCase)), Is.True,
            $"Expected error containing '{contains}' but was: {string.Join(", ", dto.Errors)}");
    }
}