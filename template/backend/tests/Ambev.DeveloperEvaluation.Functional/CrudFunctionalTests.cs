using System.Net;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

[Collection("Functional API")]
public sealed class CrudFunctionalTests(FunctionalApiFixture fixture)
{
    private static string AdminUsername =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_USERNAME") ?? "admin";

    private static string AdminPassword =>
        Environment.GetEnvironmentVariable("FUNCTIONAL_ADMIN_PASSWORD") ?? "Admin@123";

    [Fact(DisplayName = "Documented user, product and cart CRUD endpoints should be functional")]
    public async Task Should_execute_documented_crud_operations()
    {
        var adminToken = await fixture.LoginAsync(AdminUsername, AdminPassword);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var username = $"functional-crud-{suffix}";
        var userId = await fixture.CreateUserAsync(adminToken, username, "Customer");
        var productId = await fixture.CreateProductAsync(adminToken);
        var cartId = 0;

        try
        {
            var customerToken = await fixture.LoginAsync(username, "User@123");

            using var getUserResponse = await fixture.SendAsync(HttpMethod.Get, $"/api/users/{userId}", adminToken);
            Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);

            using var updateUserResponse = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/users/{userId}",
                adminToken,
                new
                {
                    username = $"{username}-updated",
                    email = $"{username}-updated@localhost.com",
                    name = new { firstname = "Updated", lastname = "Customer" },
                    address = new
                    {
                        city = "Manaus",
                        street = "Updated Street",
                        number = 20,
                        zipcode = "69000-000",
                        geolocation = new { lat = "-3.1", @long = "-60.0" }
                    },
                    phone = "+5592987777777",
                    status = "Active",
                    role = "Customer"
                });
            Assert.Equal(HttpStatusCode.OK, updateUserResponse.StatusCode);

            using var categoriesResponse = await fixture.SendAsync(HttpMethod.Get, "/api/products/categories", adminToken);
            Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);

            using var updateProductResponse = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/products/{productId}",
                adminToken,
                new
                {
                    title = $"Updated Functional Product {suffix}",
                    price = 12.5m,
                    description = "Updated functional product",
                    category = "functional-test-updated",
                    image = "http://localhost/functional-test-updated.png",
                    rating = new { rate = 4.8m, count = 2 }
                });
            Assert.Equal(HttpStatusCode.OK, updateProductResponse.StatusCode);

            using var customerCartResponse = await fixture.SendAsync(
                HttpMethod.Post,
                "/api/carts",
                customerToken,
                new
                {
                    userId,
                    date = DateTime.UtcNow,
                    products = new[] { new { productId, quantity = 1 } }
                });
            Assert.Equal(HttpStatusCode.Created, customerCartResponse.StatusCode);
            using (var cartDocument = await FunctionalApiFixture.ReadDocumentAsync(customerCartResponse))
            {
                cartId = cartDocument.RootElement.GetProperty("id").GetInt32();
            }

            using var getCartResponse = await fixture.SendAsync(HttpMethod.Get, $"/api/carts/{cartId}", customerToken);
            Assert.Equal(HttpStatusCode.OK, getCartResponse.StatusCode);

            using var updateCartResponse = await fixture.SendAsync(
                HttpMethod.Put,
                $"/api/carts/{cartId}",
                customerToken,
                new
                {
                    userId,
                    date = DateTime.UtcNow,
                    products = new[] { new { productId, quantity = 2 } }
                });
            Assert.Equal(HttpStatusCode.OK, updateCartResponse.StatusCode);
        }
        finally
        {
            if (cartId > 0)
                await fixture.DeleteAsync(adminToken, $"/api/carts/{cartId}");

            await fixture.DeleteAsync(adminToken, $"/api/products/{productId}");
            await fixture.DeleteAsync(adminToken, $"/api/users/{userId}");
        }
    }
}
