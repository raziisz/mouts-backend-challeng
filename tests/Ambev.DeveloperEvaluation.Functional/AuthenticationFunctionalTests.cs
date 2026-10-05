using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class AuthenticationFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Theory(DisplayName = "Inactive and suspended users cannot authenticate")]
    [InlineData("Inactive")]
    [InlineData("Suspended")]
    public async Task Should_reject_non_active_users(string status)
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var username = $"functional-{status.ToLowerInvariant()}-{suffix}";
        var userId = 0;

        try
        {
            using var createResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/users",
                adminToken,
                new
                {
                    username,
                    email = $"{username}@localhost.com",
                    password = "User@123",
                    name = new { firstname = "Functional", lastname = status },
                    address = new
                    {
                        city = "Manaus",
                        street = "Authentication Street",
                        number = 10,
                        zipcode = "69000-000",
                        geolocation = new { lat = "-3.1", @long = "-60.0" }
                    },
                    phone = "+5592988888888",
                    status,
                    role = "Customer"
                });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            using (var createdDocument = await FunctionalApiFixture.ReadDocumentAsync(createResponse))
            {
                userId = createdDocument.RootElement.GetProperty("id").GetInt32();
            }

            using var loginResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/auth/login",
                body: new { email = $"{username}@localhost.com", password = "User@123" });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                loginResponse,
                HttpStatusCode.Unauthorized,
                "AuthenticationError",
                "Authentication failed");
        }
        finally
        {
            if (userId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/users/{userId}");
        }
    }
}
