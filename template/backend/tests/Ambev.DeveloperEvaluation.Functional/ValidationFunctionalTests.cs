using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class ValidationFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Invalid ranges, filters and update payloads return validation errors")]
    public async Task Should_reject_invalid_query_and_update_inputs()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);

        using var invalidProductRange = await fixture.SendAsync(
            HttpMethod.Get,
            "/api/products?_minPrice=20&_maxPrice=10",
            adminToken);
        await AssertValidationErrorAsync(invalidProductRange);

        using var invalidCartRange = await fixture.SendAsync(
            HttpMethod.Get,
            "/api/carts?_minDate=2026-10-10T00%3A00%3A00Z&_maxDate=2026-10-09T00%3A00%3A00Z",
            adminToken);
        await AssertValidationErrorAsync(invalidCartRange);

        using var invalidSaleStatus = await fixture.SendAsync(
            HttpMethod.Get,
            "/api/sales?status=UnknownStatus",
            adminToken);
        await AssertValidationErrorAsync(invalidSaleStatus);

        using var invalidProductUpdate = await fixture.SendAsync(
            HttpMethod.Put,
            "/api/products/1",
            adminToken,
            new
            {
                title = "",
                price = 0,
                description = new string('x', 2001),
                category = "",
                image = "",
                rating = new { rate = 6, count = -1 }
            });
        await AssertValidationErrorAsync(invalidProductUpdate);

        var cartId = await fixture.CreateCartAsync(adminToken, 1);
        try
        {
            using var invalidCartUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/carts/{cartId}",
                adminToken,
                new
                {
                    userId = 0,
                    date = DateTime.MinValue,
                    products = new[] { new { productId = 0, quantity = 0 } }
                });
            await AssertValidationErrorAsync(invalidCartUpdate);
        }
        finally
        {
            await fixture.DeleteAsync(adminToken, $"/api/carts/{cartId}");
        }

        using var invalidSaleUpdate = await fixture.SendAsync(
            HttpMethod.Put,
            "/api/sales/1",
            adminToken,
            new
            {
                saleNumber = "",
                date = DateTime.MinValue,
                customer = new { id = 0, description = "" },
                branch = new { id = 0, description = "" },
                products = new[]
                {
                    new { productId = 0, productDescription = "", unitPrice = 0, quantity = 0 }
                }
            });
        await AssertValidationErrorAsync(invalidSaleUpdate);
    }

    private static Task AssertValidationErrorAsync(HttpResponseMessage response) =>
        FunctionalApiFixture.AssertErrorResponseAsync(
            response,
            HttpStatusCode.BadRequest,
            "ValidationError",
            "Invalid input data");
}
