using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public sealed class ErrorHandlingTests
{
    [Fact(DisplayName = "Unexpected exceptions return the standard 500 error contract")]
    public async Task Should_return_generic_internal_server_error()
    {
        var middleware = new ValidationExceptionMiddleware(
            _ => throw new Exception("Sensitive implementation detail"),
            NullLogger<ValidationExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;

        Assert.Equal("InternalServerError", root.GetProperty("type").GetString());
        Assert.Equal("Internal server error", root.GetProperty("error").GetString());
        Assert.Equal(
            "An unexpected error occurred while processing the request.",
            root.GetProperty("detail").GetString());
        Assert.DoesNotContain("Sensitive implementation detail", document.RootElement.GetRawText());
    }
}
