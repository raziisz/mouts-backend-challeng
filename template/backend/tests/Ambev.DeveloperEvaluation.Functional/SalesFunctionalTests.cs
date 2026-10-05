using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class SalesFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Sales apply discounts and support manager cancellation workflow")]
    public async Task Should_execute_sale_lifecycle()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var customerUsername = $"functional-sale-customer-{suffix}";
        var managerUsername = $"functional-sale-manager-{suffix}";
        var customerId = await fixture.CreateUserAsync(adminToken, customerUsername, "Customer");
        var managerId = await fixture.CreateUserAsync(adminToken, managerUsername, "Manager");
        var productId = await fixture.CreateProductAsync(adminToken);
        var saleId = 0;

        try
        {
            var customerToken = await fixture.LoginAsync($"{customerUsername}@localhost.com", "User@123");
            var managerToken = await fixture.LoginAsync($"{managerUsername}@localhost.com", "User@123");

            using var createResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/sales",
                customerToken,
                new
                {
                    saleNumber = $"FUNCTIONAL-SALE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "Functional customer" },
                    branch = new { id = 1, description = "Functional branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "Functional product", unitPrice = 10m, quantity = 10 }
                    }
                });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            using (var createdDocument = await FunctionalApiFixture.ReadDocumentAsync(createResponse))
            {
                var created = createdDocument.RootElement;
                saleId = created.GetProperty("id").GetInt32();
                Assert.Equal(80m, created.GetProperty("totalAmount").GetDecimal());
                Assert.Equal(0.20m, created.GetProperty("products")[0].GetProperty("discountRate").GetDecimal());
                Assert.Equal(20m, created.GetProperty("products")[0].GetProperty("discountAmount").GetDecimal());
                Assert.False(created.GetProperty("products")[0].GetProperty("isCancelled").GetBoolean());
            }

            using var customerUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/sales/{saleId}",
                customerToken,
                new
                {
                    saleNumber = $"FUNCTIONAL-SALE-UPDATE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "Functional customer" },
                    branch = new { id = 1, description = "Functional branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "Functional product", unitPrice = 10m, quantity = 4 }
                    }
                });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                customerUpdate,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");

            using var managerUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/sales/{saleId}",
                managerToken,
                new
                {
                    saleNumber = $"FUNCTIONAL-SALE-UPDATE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "Functional customer" },
                    branch = new { id = 1, description = "Functional branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "Functional product", unitPrice = 10m, quantity = 4 }
                    }
                });
            Assert.Equal(HttpStatusCode.OK, managerUpdate.StatusCode);

            int itemId;
            using (var updatedDocument = await FunctionalApiFixture.ReadDocumentAsync(managerUpdate))
            {
                var updated = updatedDocument.RootElement;
                Assert.Equal(36m, updated.GetProperty("totalAmount").GetDecimal());
                Assert.Equal(0.10m, updated.GetProperty("products")[0].GetProperty("discountRate").GetDecimal());
                itemId = updated.GetProperty("products")[0].GetProperty("id").GetInt32();
            }

            using var cancelItemResponse = await fixture.SendAsync(
                HttpMethod.Patch,
                $"/api/sales/{saleId}/items/{itemId}/cancel",
                managerToken);
            Assert.Equal(HttpStatusCode.OK, cancelItemResponse.StatusCode);
            using (var cancelledItemDocument = await FunctionalApiFixture.ReadDocumentAsync(cancelItemResponse))
            {
                var cancelled = cancelledItemDocument.RootElement;
                Assert.Equal(0m, cancelled.GetProperty("totalAmount").GetDecimal());
                Assert.True(cancelled.GetProperty("products")[0].GetProperty("isCancelled").GetBoolean());
            }

            using var cancelSaleResponse = await fixture.SendAsync(
                HttpMethod.Delete,
                $"/api/sales/{saleId}",
                managerToken);
            Assert.Equal(HttpStatusCode.OK, cancelSaleResponse.StatusCode);
            using (var cancelledSaleDocument = await FunctionalApiFixture.ReadDocumentAsync(cancelSaleResponse))
            {
                var cancelledSale = cancelledSaleDocument.RootElement;
                Assert.Equal("Cancelled", cancelledSale.GetProperty("status").GetString());
                Assert.Equal(0m, cancelledSale.GetProperty("totalAmount").GetDecimal());
            }

            using var excessiveQuantityResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/sales",
                customerToken,
                new
                {
                    saleNumber = $"FUNCTIONAL-SALE-INVALID-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "Functional customer" },
                    branch = new { id = 1, description = "Functional branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "Functional product", unitPrice = 10m, quantity = 21 }
                    }
                });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                excessiveQuantityResponse,
                HttpStatusCode.Conflict,
                "BusinessRuleViolation",
                "Business rule violation");
        }
        finally
        {
            if (saleId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/sales/{saleId}");

            await fixture.DeleteAsync(adminToken, $"/api/products/{productId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{customerId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{managerId}");
        }
    }
}
