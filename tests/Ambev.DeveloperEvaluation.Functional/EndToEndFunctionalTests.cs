using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class EndToEndFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Catalog, cart and manager sale journey should work end to end")]
    public async Task Should_complete_catalog_cart_and_manager_sale_journey()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var customerUsername = $"e2e-customer-{suffix}";
        var managerUsername = $"e2e-manager-{suffix}";
        var customerId = await fixture.CreateUserAsync(adminToken, customerUsername, "Customer");
        var managerId = await fixture.CreateUserAsync(adminToken, managerUsername, "Manager");
        var productId = await fixture.CreateProductAsync(adminToken);
        var cartId = 0;
        var saleId = 0;

        try
        {
            var customerToken = await fixture.LoginAsync($"{customerUsername}@localhost.com", "User@123");
            var managerToken = await fixture.LoginAsync($"{managerUsername}@localhost.com", "User@123");

            using var productList = await fixture.SendAsync(HttpMethod.Get, "/api/products", customerToken);
            Assert.Equal(HttpStatusCode.OK, productList.StatusCode);

            using var productDetails = await fixture.SendAsync(HttpMethod.Get, $"/api/products/{productId}", customerToken);
            Assert.Equal(HttpStatusCode.OK, productDetails.StatusCode);

            using var categoryProducts = await fixture.SendAsync(HttpMethod.Get, "/api/products/category/functional-test", customerToken);
            Assert.Equal(HttpStatusCode.OK, categoryProducts.StatusCode);
            using (var categoryDocument = await FunctionalApiFixture.ReadDocumentAsync(categoryProducts))
            {
                var productIds = categoryDocument.RootElement.GetProperty("data")
                    .EnumerateArray()
                    .Select(product => product.GetProperty("id").GetInt32())
                    .ToArray();
                Assert.Contains(productId, productIds);
            }

            using var createCart = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/carts",
                customerToken,
                new
                {
                    userId = customerId,
                    date = DateTime.UtcNow,
                    products = new[] { new { productId, quantity = 1 } }
                });
            Assert.Equal(HttpStatusCode.Created, createCart.StatusCode);
            using (var cartDocument = await FunctionalApiFixture.ReadDocumentAsync(createCart))
            {
                cartId = cartDocument.RootElement.GetProperty("id").GetInt32();
            }

            using var customerCarts = await fixture.SendAsync(HttpMethod.Get, "/api/carts", customerToken);
            Assert.Equal(HttpStatusCode.OK, customerCarts.StatusCode);

            using var updateCart = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/carts/{cartId}",
                customerToken,
                new
                {
                    userId = customerId,
                    date = DateTime.UtcNow,
                    products = new[] { new { productId, quantity = 2 } }
                });
            Assert.Equal(HttpStatusCode.OK, updateCart.StatusCode);

            using var createSale = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/sales",
                managerToken,
                new
                {
                    saleNumber = $"E2E-SALE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "E2E customer" },
                    branch = new { id = 1, description = "E2E branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "E2E product", unitPrice = 10m, quantity = 4 }
                    }
                });
            Assert.Equal(HttpStatusCode.Created, createSale.StatusCode);
            using (var saleDocument = await FunctionalApiFixture.ReadDocumentAsync(createSale))
            {
                var sale = saleDocument.RootElement;
                saleId = sale.GetProperty("id").GetInt32();
                Assert.Equal(36m, sale.GetProperty("totalAmount").GetDecimal());
                Assert.Equal(0.10m, sale.GetProperty("products")[0].GetProperty("discountRate").GetDecimal());
            }

            using var customerSales = await fixture.SendAsync(HttpMethod.Get, "/api/sales", customerToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                customerSales,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");

            using var customerSale = await fixture.SendAsync(HttpMethod.Get, $"/api/sales/{saleId}", customerToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                customerSale,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");

            using var forbiddenUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/sales/{saleId}",
                customerToken,
                new
                {
                    saleNumber = $"E2E-SALE-CUSTOMER-UPDATE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "E2E customer" },
                    branch = new { id = 1, description = "E2E branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "E2E product", unitPrice = 10m, quantity = 1 }
                    }
                });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                forbiddenUpdate,
                HttpStatusCode.Forbidden,
                "AuthorizationError",
                "Access denied");

            using var managerUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/sales/{saleId}",
                managerToken,
                new
                {
                    saleNumber = $"E2E-SALE-MANAGER-UPDATE-{suffix}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerId, description = "E2E customer" },
                    branch = new { id = 1, description = "E2E branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "E2E product", unitPrice = 10m, quantity = 1 }
                    }
                });
            Assert.Equal(HttpStatusCode.OK, managerUpdate.StatusCode);

            int itemId;
            using (var updatedSaleDocument = await FunctionalApiFixture.ReadDocumentAsync(managerUpdate))
            {
                itemId = updatedSaleDocument.RootElement.GetProperty("products")[0].GetProperty("id").GetInt32();
            }

            using var cancelItem = await fixture.SendAsync(
                HttpMethod.Patch,
                $"/api/sales/{saleId}/items/{itemId}/cancel",
                managerToken);
            Assert.Equal(HttpStatusCode.OK, cancelItem.StatusCode);

            using var cancelSale = await fixture.SendAsync(
                HttpMethod.Delete,
                $"/api/sales/{saleId}",
                managerToken);
            Assert.Equal(HttpStatusCode.OK, cancelSale.StatusCode);
            using (var cancelledSaleDocument = await FunctionalApiFixture.ReadDocumentAsync(cancelSale))
            {
                Assert.Equal("Cancelled", cancelledSaleDocument.RootElement.GetProperty("status").GetString());
                Assert.Equal(0m, cancelledSaleDocument.RootElement.GetProperty("totalAmount").GetDecimal());
            }
        }
        finally
        {
            if (saleId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/sales/{saleId}");
            if (cartId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/carts/{cartId}");

            await fixture.DeleteAsync(adminToken, $"/api/products/{productId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{customerId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{managerId}");
        }
    }
}
