using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public sealed class FunctionalApiFixture : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseUrl = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_BASE_URL") ?? "http://localhost:8080";
        Client = new HttpClient { BaseAddress = new Uri(baseUrl) };

        using var response = await Client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        string? token = null,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        return await Client.SendAsync(request);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/auth/login", body: new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("token").GetString()!;
    }

    public async Task<int> CreateUserAsync(string adminToken, string username, string role)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/users", adminToken, new
        {
            username,
            email = $"{username}@localhost.com",
            password = "User@123",
            name = new { firstname = "Functional", lastname = role },
            address = new
            {
                city = "Manaus",
                street = "Functional Street",
                number = 10,
                zipcode = "69000-000",
                geolocation = new { lat = "-3.1", @long = "-60.0" }
            },
            phone = "+5592988888888",
            status = "Active",
            role
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    public async Task<int> CreateProductAsync(string token)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/products", token, new
        {
            title = $"Functional Product {Guid.NewGuid():N}",
            price = 10,
            description = "Functional test product",
            category = "functional-test",
            image = "http://localhost/functional-test.png",
            rating = new { rate = 4.5m, count = 1 }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    public async Task<int> CreateCartAsync(string token, int userId)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/carts", token, new
        {
            userId,
            date = DateTime.UtcNow,
            products = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    public async Task<int> CreateSaleAsync(string token, int customerId, int productId)
    {
        var saleNumber = $"FUNCTIONAL-{Guid.NewGuid():N}";
        using var response = await SendAsync(HttpMethod.Post, "/api/sales", token, new
        {
            saleNumber,
            date = DateTime.UtcNow,
            customer = new { id = customerId, description = "Functional customer" },
            branch = new { id = 1, description = "Functional branch" },
            products = new[]
            {
                new { productId, productDescription = "Functional product", unitPrice = 10, quantity = 1 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetInt32();
    }

    public async Task DeleteAsync(string token, string path, HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, token);
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    public static async Task<JsonDocument> ReadDocumentAsync(HttpResponseMessage response)
    {
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }

    public static async Task AssertErrorResponseAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedType,
        string expectedError)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        using var document = await ReadDocumentAsync(response);
        var root = document.RootElement;

        Assert.Equal(expectedType, root.GetProperty("type").GetString());
        Assert.Equal(expectedError, root.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
    }
}

[CollectionDefinition("Functional API", DisableParallelization = true)]
public sealed class FunctionalApiCollection : ICollectionFixture<FunctionalApiFixture>
{
}
