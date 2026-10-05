using System.Net;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class EndpointCoverageFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminEmail =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_EMAIL") ?? "admin@localhost";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Authentication rejects invalid credentials and invalid payloads")]
    public async Task Should_validate_login_requests()
    {
        using var invalidCredentials = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/auth/login",
            body: new { email = AdminEmail, password = "Wrong@123" });
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidCredentials,
            HttpStatusCode.Unauthorized,
            "AuthenticationError",
            "Authentication failed");

        using var invalidPayload = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/auth/login",
            body: new { email = "invalid-email", password = "" });
        await FunctionalApiFixture.AssertErrorResponseAsync(
            invalidPayload,
            HttpStatusCode.BadRequest,
            "ValidationError",
            "Invalid input data");
    }

    [Fact(DisplayName = "Product category, filter and pagination endpoints should enforce their contracts")]
    public async Task Should_filter_products_and_validate_pagination()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var category = $"coverage-category-{suffix}";
        var firstProductId = await CreateProductAsync(adminToken, $"Coverage Product A {suffix}", 10, category);
        var secondProductId = await CreateProductAsync(adminToken, $"Coverage Product B {suffix}", 25, category);

        try
        {
            using var categoryResponse = await fixture.SendAsync(
                HttpMethod.Get,
                $"/api/products/category/{category}",
                adminToken);
            Assert.Equal(HttpStatusCode.OK, categoryResponse.StatusCode);
            using (var categoryDocument = await FunctionalApiFixture.ReadDocumentAsync(categoryResponse))
            {
                var ids = ReadIds(categoryDocument);
                Assert.Contains(firstProductId, ids);
                Assert.Contains(secondProductId, ids);
            }

            using var filteredResponse = await fixture.SendAsync(
                HttpMethod.Get,
                $"/api/products?category={category}&_minPrice=20&_maxPrice=30&_order=price%20desc&_page=1&_size=1",
                adminToken);
            Assert.Equal(HttpStatusCode.OK, filteredResponse.StatusCode);
            using (var filteredDocument = await FunctionalApiFixture.ReadDocumentAsync(filteredResponse))
            {
                var root = filteredDocument.RootElement;
                Assert.Equal(1, root.GetProperty("totalItems").GetInt32());
                Assert.Equal(secondProductId, root.GetProperty("data")[0].GetProperty("id").GetInt32());
            }

            using var invalidPage = await fixture.SendAsync(
                HttpMethod.Get,
                "/api/products?_page=0",
                adminToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                invalidPage,
                HttpStatusCode.BadRequest,
                "ValidationError",
                "Invalid input data");

            using var invalidSize = await fixture.SendAsync(
                HttpMethod.Get,
                "/api/products?_size=101",
                adminToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                invalidSize,
                HttpStatusCode.BadRequest,
                "ValidationError",
                "Invalid input data");
        }
        finally
        {
            await fixture.DeleteAsync(adminToken, $"/api/products/{firstProductId}");
            await fixture.DeleteAsync(adminToken, $"/api/products/{secondProductId}");
        }
    }

    [Fact(DisplayName = "User filters and duplicate identifiers should return the documented responses")]
    public async Task Should_filter_users_and_reject_duplicate_identifiers()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var firstUsername = $"coverage-user-a-{suffix}";
        var secondUsername = $"coverage-user-b-{suffix}";
        var firstEmail = $"{firstUsername}@localhost.com";
        var firstUserId = await fixture.CreateUserAsync(adminToken, firstUsername, "Customer");
        var secondUserId = await fixture.CreateUserAsync(adminToken, secondUsername, "Customer");

        try
        {
            using var invalidAddressCreate = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/users",
                adminToken,
                UserBody($"coverage-invalid-address-{suffix}", $"coverage-invalid-address-{suffix}@localhost.com", "+5592987777778", addressNumber: 0));
            await FunctionalApiFixture.AssertErrorResponseAsync(
                invalidAddressCreate,
                HttpStatusCode.BadRequest,
                "ValidationError",
                "Invalid input data");

            using var filteredUsers = await fixture.SendAsync(
                HttpMethod.Get,
                $"/api/users?username={Uri.EscapeDataString(firstUsername + "*")}&email={Uri.EscapeDataString(firstEmail)}&phone=%2B5592988888888&status=Active&role=Customer&_order=username%20asc",
                adminToken);
            Assert.Equal(HttpStatusCode.OK, filteredUsers.StatusCode);
            using (var usersDocument = await FunctionalApiFixture.ReadDocumentAsync(filteredUsers))
            {
                var ids = ReadIds(usersDocument);
                Assert.Equal([firstUserId], ids);
            }

            using var duplicateCreate = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/users",
                adminToken,
                UserBody($"coverage-duplicate-{suffix}", firstEmail, "+5592987777777"));
            await FunctionalApiFixture.AssertErrorResponseAsync(
                duplicateCreate,
                HttpStatusCode.Conflict,
                "Conflict",
                "Operation cannot be completed");

            using var duplicateUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/users/{secondUserId}",
                adminToken,
                UserBody(secondUsername, firstEmail, "+5592987777777"));
            await FunctionalApiFixture.AssertErrorResponseAsync(
                duplicateUpdate,
                HttpStatusCode.Conflict,
                "Conflict",
                "Operation cannot be completed");
        }
        finally
        {
            await fixture.DeleteAsync(adminToken, $"/api/users/{firstUserId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{secondUserId}");
        }
    }

    [Fact(DisplayName = "Privileged users can filter carts and sales across customers")]
    public async Task Should_filter_carts_and_sales_and_handle_missing_mutations()
    {
        var adminToken = await fixture.LoginAsync(AdminEmail, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var customerUsername = $"coverage-customer-{suffix}";
        var managerUsername = $"coverage-manager-{suffix}";
        var customerId = await fixture.CreateUserAsync(adminToken, customerUsername, "Customer");
        var managerId = await fixture.CreateUserAsync(adminToken, managerUsername, "Manager");
        var productId = await CreateProductAsync(adminToken, $"Coverage Sale Product {suffix}", 15, "coverage-sales");
        var cartId = 0;
        var saleId = 0;
        var cartDate = DateTime.UtcNow.Date.AddDays(-1);
        var saleDate = DateTime.UtcNow.Date;

        try
        {
            var customerToken = await fixture.LoginAsync($"{customerUsername}@localhost.com", "User@123");
            var managerToken = await fixture.LoginAsync($"{managerUsername}@localhost.com", "User@123");

            cartId = await CreateCartAsync(customerToken, customerId, cartDate);
            saleId = await CreateSaleAsync(managerToken, customerId, productId, $"COVERAGE-SALE-{suffix}", saleDate);

            using var filteredCarts = await fixture.SendAsync(
                HttpMethod.Get,
                $"/api/carts?userId={customerId}&date={Uri.EscapeDataString(cartDate.ToString("O"))}&_minDate={Uri.EscapeDataString(cartDate.AddDays(-1).ToString("O"))}&_maxDate={Uri.EscapeDataString(cartDate.AddDays(1).ToString("O"))}&_order=date%20desc",
                adminToken);
            Assert.Equal(HttpStatusCode.OK, filteredCarts.StatusCode);
            Assert.Contains(cartId, ReadIds(await FunctionalApiFixture.ReadDocumentAsync(filteredCarts)));

            using var filteredSales = await fixture.SendAsync(
                HttpMethod.Get,
                $"/api/sales?saleNumber={Uri.EscapeDataString($"COVERAGE-SALE-{suffix}")}&status=Active&date={Uri.EscapeDataString(saleDate.ToString("O"))}&_minDate={Uri.EscapeDataString(saleDate.AddDays(-1).ToString("O"))}&_maxDate={Uri.EscapeDataString(saleDate.AddDays(1).ToString("O"))}&_order=date%20desc",
                adminToken);
            Assert.Equal(HttpStatusCode.OK, filteredSales.StatusCode);
            Assert.Contains(saleId, ReadIds(await FunctionalApiFixture.ReadDocumentAsync(filteredSales)));

            using var managerCart = await fixture.SendAsync(HttpMethod.Get, $"/api/carts/{cartId}", managerToken);
            Assert.Equal(HttpStatusCode.OK, managerCart.StatusCode);

            using var managerSale = await fixture.SendAsync(HttpMethod.Get, $"/api/sales/{saleId}", managerToken);
            Assert.Equal(HttpStatusCode.OK, managerSale.StatusCode);

            using var missingCartUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/carts/{int.MaxValue}",
                adminToken,
                new { userId = customerId, date = DateTime.UtcNow, products = Array.Empty<object>() });
            await FunctionalApiFixture.AssertErrorResponseAsync(
                missingCartUpdate,
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found");

            using var missingSaleUpdate = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/sales/{int.MaxValue}",
                managerToken,
                SaleBody($"COVERAGE-MISSING-{suffix}", customerId, productId));
            await FunctionalApiFixture.AssertErrorResponseAsync(
                missingSaleUpdate,
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found");

            using var missingSaleDelete = await fixture.SendAsync(
                HttpMethod.Delete,
                $"/api/sales/{int.MaxValue}",
                managerToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                missingSaleDelete,
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found");

            using var missingSaleItemCancel = await fixture.SendAsync(
                HttpMethod.Patch,
                $"/api/sales/{int.MaxValue}/items/{int.MaxValue}/cancel",
                managerToken);
            await FunctionalApiFixture.AssertErrorResponseAsync(
                missingSaleItemCancel,
                HttpStatusCode.NotFound,
                "ResourceNotFound",
                "Resource not found");
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

    private async Task<int> CreateProductAsync(string token, string title, decimal price, string category)
    {
        using var response = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/products",
            token,
            new
            {
                title,
                price,
                description = "HTTP coverage product",
                category,
                image = "http://localhost/coverage.png",
                rating = new { rate = 4.5m, count = 1 }
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await FunctionalApiFixture.ReadDocumentAsync(response);
        return document.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CreateCartAsync(string token, int userId, DateTime date)
    {
        using var response = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/carts",
            token,
            new { userId, date, products = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await FunctionalApiFixture.ReadDocumentAsync(response);
        return document.RootElement.GetProperty("id").GetInt32();
    }

    private async Task<int> CreateSaleAsync(string token, int customerId, int productId, string saleNumber, DateTime date)
    {
        using var response = await fixture.SendAsync(
            HttpMethod.Post,
            "/api/sales",
            token,
            SaleBody(saleNumber, customerId, productId, date));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await FunctionalApiFixture.ReadDocumentAsync(response);
        return document.RootElement.GetProperty("id").GetInt32();
    }

    private static object UserBody(string username, string email, string phone, int addressNumber = 10) => new
    {
        username,
        email,
        password = "User@123",
        name = new { firstname = "Coverage", lastname = "User" },
        address = new
        {
            city = "Manaus",
            street = "Coverage Street",
            number = addressNumber,
            zipcode = "69000-000",
            geolocation = new { lat = "-3.1", @long = "-60.0" }
        },
        phone,
        status = "Active",
        role = "Customer"
    };

    private static object SaleBody(string saleNumber, int customerId, int productId, DateTime? date = null) => new
    {
        saleNumber,
        date = date ?? DateTime.UtcNow,
        customer = new { id = customerId, description = "Coverage customer" },
        branch = new { id = 1, description = "Coverage branch" },
        products = new[]
        {
            new { productId, productDescription = "Coverage product", unitPrice = 15m, quantity = 1 }
        }
    };

    private static int[] ReadIds(JsonDocument document) => document.RootElement
        .GetProperty("data")
        .EnumerateArray()
        .Select(item => item.GetProperty("id").GetInt32())
        .ToArray();
}
