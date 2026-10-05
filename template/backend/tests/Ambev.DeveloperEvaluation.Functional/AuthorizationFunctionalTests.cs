using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class AuthorizationFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminUsername =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_USERNAME") ?? "admin";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Anonymous users must be rejected and roles must follow the authorization matrix")]
    public async Task Should_enforce_role_permissions()
    {
        var anonymousResponse = await fixture.SendAsync(HttpMethod.Get, "/api/products");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var adminToken = await fixture.LoginAsync(AdminUsername, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var customerUsername = $"functional-customer-{suffix}";
        var managerUsername = $"functional-manager-{suffix}";
        var customerId = await fixture.CreateUserAsync(adminToken, customerUsername, "Customer");
        var managerId = await fixture.CreateUserAsync(adminToken, managerUsername, "Manager");
        var productId = 0;

        try
        {
            var customerToken = await fixture.LoginAsync(customerUsername, "User@123");
            var managerToken = await fixture.LoginAsync(managerUsername, "User@123");

            using var customerProducts = await fixture.SendAsync(HttpMethod.Get, "/api/products", customerToken);
            Assert.Equal(HttpStatusCode.OK, customerProducts.StatusCode);

            using var customerUsers = await fixture.SendAsync(HttpMethod.Get, "/api/users", customerToken);
            Assert.Equal(HttpStatusCode.Forbidden, customerUsers.StatusCode);

            using var customerProductCreate = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/products",
                customerToken,
                new
                {
                    title = "Forbidden product",
                    price = 10,
                    description = "Should be rejected",
                    category = "functional-test",
                    image = "http://localhost/forbidden.png",
                    rating = new { rate = 1, count = 0 }
                });
            Assert.Equal(HttpStatusCode.Forbidden, customerProductCreate.StatusCode);

            using var managerUsers = await fixture.SendAsync(HttpMethod.Get, "/api/users", managerToken);
            Assert.Equal(HttpStatusCode.Forbidden, managerUsers.StatusCode);

            using var managerProductCreate = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/products",
                managerToken,
                new
                {
                    title = "Forbidden manager product",
                    price = 10,
                    description = "Should be rejected",
                    category = "functional-test",
                    image = "http://localhost/forbidden-manager.png",
                    rating = new { rate = 1, count = 0 }
                });
            Assert.Equal(HttpStatusCode.Forbidden, managerProductCreate.StatusCode);

            using var managerSales = await fixture.SendAsync(HttpMethod.Get, "/api/sales", managerToken);
            Assert.Equal(HttpStatusCode.OK, managerSales.StatusCode);

            productId = await fixture.CreateProductAsync(adminToken);
            using var adminProduct = await fixture.SendAsync(HttpMethod.Get, $"/api/products/{productId}", adminToken);
            Assert.Equal(HttpStatusCode.OK, adminProduct.StatusCode);
        }
        finally
        {
            if (productId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/products/{productId}");

            await fixture.DeleteAsync(adminToken, $"/api/users/{customerId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{managerId}");
        }
    }

    [Fact(DisplayName = "Customers must only access their own carts and sales")]
    public async Task Should_isolate_customer_purchases()
    {
        var adminToken = await fixture.LoginAsync(AdminUsername, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var customerAUsername = $"functional-owner-{suffix}";
        var customerBUsername = $"functional-other-{suffix}";
        var customerAId = await fixture.CreateUserAsync(adminToken, customerAUsername, "Customer");
        var customerBId = await fixture.CreateUserAsync(adminToken, customerBUsername, "Customer");
        var productId = await fixture.CreateProductAsync(adminToken);
        var cartAId = 0;
        var cartBId = 0;
        var saleAId = 0;
        var saleBId = 0;

        try
        {
            var customerAToken = await fixture.LoginAsync(customerAUsername, "User@123");
            var customerBToken = await fixture.LoginAsync(customerBUsername, "User@123");

            cartAId = await fixture.CreateCartAsync(customerAToken, customerAId);
            cartBId = await fixture.CreateCartAsync(customerBToken, customerBId);
            saleAId = await fixture.CreateSaleAsync(customerAToken, customerAId, productId);
            saleBId = await fixture.CreateSaleAsync(customerBToken, customerBId, productId);

            using var customerACarts = await fixture.SendAsync(HttpMethod.Get, "/api/carts", customerAToken);
            Assert.Equal(HttpStatusCode.OK, customerACarts.StatusCode);
            using (var cartsDocument = await FunctionalApiFixture.ReadDocumentAsync(customerACarts))
            {
                var cartIds = cartsDocument.RootElement.GetProperty("data")
                    .EnumerateArray()
                    .Select(cart => cart.GetProperty("id").GetInt32())
                    .ToArray();
                Assert.Contains(cartAId, cartIds);
                Assert.DoesNotContain(cartBId, cartIds);
            }

            using var customerASales = await fixture.SendAsync(HttpMethod.Get, "/api/sales", customerAToken);
            Assert.Equal(HttpStatusCode.OK, customerASales.StatusCode);
            using (var salesDocument = await FunctionalApiFixture.ReadDocumentAsync(customerASales))
            {
                var salesCustomerIds = salesDocument.RootElement.GetProperty("data")
                    .EnumerateArray()
                    .Select(sale => sale.GetProperty("customer").GetProperty("id").GetInt32())
                    .ToArray();
                Assert.Contains(customerAId, salesCustomerIds);
                Assert.DoesNotContain(customerBId, salesCustomerIds);
            }

            using var otherCart = await fixture.SendAsync(HttpMethod.Get, $"/api/carts/{cartBId}", customerAToken);
            Assert.Equal(HttpStatusCode.Forbidden, otherCart.StatusCode);

            using var otherSale = await fixture.SendAsync(HttpMethod.Get, $"/api/sales/{saleBId}", customerAToken);
            Assert.Equal(HttpStatusCode.Forbidden, otherSale.StatusCode);

            using var foreignCartCreation = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/carts",
                customerAToken,
                new { userId = customerBId, date = DateTime.UtcNow, products = Array.Empty<object>() });
            Assert.Equal(HttpStatusCode.Forbidden, foreignCartCreation.StatusCode);

            using var foreignSaleCreation = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/sales",
                customerAToken,
                new
                {
                    saleNumber = $"FUNCTIONAL-FORBIDDEN-{Guid.NewGuid():N}",
                    date = DateTime.UtcNow,
                    customer = new { id = customerBId, description = "Foreign customer" },
                    branch = new { id = 1, description = "Functional branch" },
                    products = new[]
                    {
                        new { productId, productDescription = "Functional product", unitPrice = 10, quantity = 1 }
                    }
                });
            Assert.Equal(HttpStatusCode.Forbidden, foreignSaleCreation.StatusCode);
        }
        finally
        {
            if (saleAId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/sales/{saleAId}");
            if (saleBId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/sales/{saleBId}");
            if (cartAId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/carts/{cartAId}");
            if (cartBId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/carts/{cartBId}");

            await fixture.DeleteAsync(adminToken, $"/api/products/{productId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{customerAId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{customerBId}");
        }
    }
}
