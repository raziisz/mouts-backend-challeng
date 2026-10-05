using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.ORM.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public sealed class MongoEventPublisherIntegrationTests
{
    private const string DefaultConnectionString =
        "mongodb://developer_test:ev%40luAt10n_test@localhost:27018/?authSource=admin";

    [Fact]
    public async Task Sale_created_event_should_be_persisted_in_mongodb()
    {
        var connectionString = Environment.GetEnvironmentVariable("INTEGRATION_TEST_MONGODB_CONNECTION_STRING")
            ?? DefaultConnectionString;
        var databaseName = $"dev_eval_evt_{Guid.NewGuid():N}";
        var configuration = new ConfigurationManager
        {
            ["ConnectionStrings:MongoDb"] = connectionString,
            ["MongoDb:Database"] = databaseName
        };
        var client = new MongoClient(connectionString);
        var collection = client
            .GetDatabase(databaseName)
            .GetCollection<BsonDocument>("domain_events");

        try
        {
            var sale = new Sale
            {
                SaleNumber = $"MONGO-{Guid.NewGuid():N}",
                Date = DateTime.UtcNow,
                Customer = new ExternalIdentity { Id = 42, Description = "Integration customer" },
                Branch = new ExternalIdentity { Id = 7, Description = "Integration branch" }
            };
            sale.AddItem(1, "Integration product", 10m, 1);

            var publisher = new MongoEventPublisher(
                configuration,
                NullLogger<MongoEventPublisher>.Instance);

            await publisher.PublishAsync(new SaleCreated(sale));

            var persistedEvent = await collection
                .Find(Builders<BsonDocument>.Filter.Eq("eventType", nameof(SaleCreated)))
                .FirstOrDefaultAsync();

            Assert.NotNull(persistedEvent);
            Assert.Equal(nameof(SaleCreated), persistedEvent["eventType"].AsString);
            Assert.True(persistedEvent.Contains("occurredAt"));
            Assert.True(persistedEvent.Contains("payload"));
        }
        finally
        {
            await client.DropDatabaseAsync(databaseName);
        }
    }
}
