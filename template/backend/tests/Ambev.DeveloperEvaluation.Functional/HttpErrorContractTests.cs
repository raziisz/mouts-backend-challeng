using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class HttpErrorContractTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Invalid catalog payloads return the standard 400 validation contract")]
    public async Task Should_reject_invalid_product_cart_and_sale_payloads()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);

        using var invalidProduct = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/products",
            adminToken,
            new
            {
                title = "",
                price = 0,
                description = "",
                category = "",
                image = "",
                rating = new { rate = 6, count = -1 }
            });
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidProduct,
            HttpStatusCode.BadRequest,
            "ValidationError",
            "Invalid input data");

        using var invalidCart = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/carts",
            adminToken,
            new
            {
                userId = 0,
                date = DateTime.MinValue,
                products = new[] { new { productId = 0, quantity = 0 } }
            });
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidCart,
            HttpStatusCode.BadRequest,
            "ValidationError",
            "Invalid input data");

        using var invalidSale = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/sales",
            adminToken,
            new
            {
                saleNumber = "",
                date = DateTime.MinValue,
                customer = new { id = 0, description = "" },
                branch = new { id = 0, description = "" },
                products = new[]
                {
                    new
                    {
                        productId = 0,
                        productDescription = "",
                        unitPrice = 0,
                        quantity = 0
                    }
                }
            });
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidSale,
            HttpStatusCode.BadRequest,
            "ValidationError",
            "Invalid input data");
    }

    [Fact(DisplayName = "Missing or invalid credentials return the standard 401 contract")]
    public async Task Should_reject_anonymous_and_invalid_token_requests()
    {
        using var anonymousResponse = await fixture.SendAsync(HttpMethod.Get, "/api/products");
        await FunctionalApiFixture.AssertErrorResponseAsync(
            anonymousResponse,
            HttpStatusCode.Unauthorized,
            "AuthenticationError",
            "Authentication failed");

        using var invalidTokenResponse = await fixture.SendAsync(
            HttpMethod.Get,
            "/api/products",
            "invalid.jwt.token");
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidTokenResponse,
            HttpStatusCode.Unauthorized,
            "AuthenticationError",
            "Authentication failed");
    }

    [Fact(DisplayName = "Authenticated users without the required role return the standard 403 contract")]
    public async Task Should_reject_customer_access_to_admin_routes()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var username = $"functional-http-customer-{Guid.NewGuid():N}"[..30];
        var userId = await fixture.CreateUserAsync(adminToken, username, "Customer");

        try
        {
            var customerToken = await fixture.LoginAsync($"{username}@localhost.com", "User@123");

            using var usersResponse = await fixture.SendAsync(HttpMethod.Get, "/api/users", customerToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                usersResponse,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");

            using var productsResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/products",
                customerToken,
                new
                {
                    title = "Unauthorized product",
                    price = 10,
                    description = "Should not be created",
                    category = "functional-test",
                    image = "http://localhost/unauthorized.png",
                    rating = new { rate = 1, count = 0 }
                });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                productsResponse,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");
        }
        finally
        {
            await fixture.DeleteAsync(adminToken, $"/api/users/{userId}");
        }
    }

    [Fact(DisplayName = "Unknown documented resources return the standard 404 contract")]
    public async Task Should_return_not_found_for_unknown_resources()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        const int unknownId = int.MaxValue;

        foreach (var path in new[]
        {
            $"/api/users/{unknownId}",
            $"/api/products/{unknownId}",
            $"/api/carts/{unknownId}",
            $"/api/sales/{unknownId}"
        })
        {
            using var response = await fixture.SendAsync(HttpMethod.Get, path, adminToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                response,
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found");
        }
    }
}
