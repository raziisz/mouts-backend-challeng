using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact(DisplayName = "Unexpected exceptions return the standard 500 contract over HTTP")]
    public async Task Should_return_generic_internal_server_error_over_http()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        await using var app = builder.Build();
        app.Use(next => context => new ValidationExceptionMiddleware(
            next,
            NullLogger<ValidationExceptionMiddleware>.Instance).InvokeAsync(context));
        app.Run(_ => throw new Exception("Sensitive implementation detail"));
        await app.StartAsync();

        try
        {
            var server = app.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            using var response = await client.GetAsync("/");

            Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = document.RootElement;

            Assert.Equal("InternalServerError", root.GetProperty("type").GetString());
            Assert.Equal("Internal server error", root.GetProperty("error").GetString());
            Assert.Equal(
                "An unexpected error occurred while processing the request.",
                root.GetProperty("detail").GetString());
            Assert.DoesNotContain("Sensitive implementation detail", document.RootElement.GetRawText());
        }
        finally
        {
            await app.StopAsync();
        }
    }

}
