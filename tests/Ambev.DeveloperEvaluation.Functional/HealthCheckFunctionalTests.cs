using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class HealthCheckFunctionalTests(FunctionalApiFixture fixture)
{
    [Fact(DisplayName = "Overall health returns all registered checks")]
    public async Task Should_return_overall_health_with_registered_checks()
    {
        using var response = await fixture.SendAsync(HttpMethod.Get, "/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await FunctionalApiFixture.ReadDocumentAsync(response);
        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        var checkNames = root.GetProperty("healthChecks")
            .EnumerateArray()
            .Select(check => check.GetProperty("name").GetString() ?? string.Empty)
            .ToArray();

        Assert.Equal(["Liveness", "Readiness"], checkNames);
        Assert.All(root.GetProperty("healthChecks").EnumerateArray(), check =>
        {
            Assert.Equal("Healthy", check.GetProperty("status").GetString());
            Assert.True(check.GetProperty("hostEnvironment").GetString() is not null);
        });
    }

    [Theory(DisplayName = "Health probes return their specific registered check")]
    [InlineData("/health/live", "Liveness")]
    [InlineData("/health/ready", "Readiness")]
    public async Task Should_return_specific_health_probe(string path, string expectedCheckName)
    {
        using var response = await fixture.SendAsync(HttpMethod.Get, path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await FunctionalApiFixture.ReadDocumentAsync(response);
        var root = document.RootElement;
        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        var checks = root.GetProperty("healthChecks").EnumerateArray().ToArray();
        var check = Assert.Single(checks);
        Assert.Equal(expectedCheckName, check.GetProperty("name").GetString());
        Assert.Equal("Healthy", check.GetProperty("status").GetString());
    }
}
