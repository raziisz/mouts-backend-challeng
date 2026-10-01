using Ambev.DeveloperEvaluation.Domain.Events;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Events;

public class MongoEventPublisher : IEventPublisher
{
    private readonly IMongoCollection<BsonDocument> _collection;
    private readonly ILogger<MongoEventPublisher> _logger;

    public MongoEventPublisher(IConfiguration configuration, ILogger<MongoEventPublisher> logger)
    {
        _logger = logger;
        var connection = configuration.GetConnectionString("MongoDb") ?? "mongodb://developer:ev%40luAt10n@localhost:27017/?authSource=admin";
        var databaseName = configuration["MongoDb:Database"] ?? "developer_evaluation";
        var database = new MongoClient(connection).GetDatabase(databaseName);
        _collection = database.GetCollection<BsonDocument>("domain_events");
    }

    public async Task PublishAsync(ISaleEvent saleEvent, CancellationToken cancellationToken = default)
    {
        var document = new BsonDocument
        {
            ["eventType"] = saleEvent.GetType().Name,
            ["occurredAt"] = DateTime.UtcNow,
            ["payload"] = BsonDocument.Parse(JsonSerializer.Serialize(saleEvent))
        };
        try
        {
            await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not persist domain event {EventType} in MongoDB", saleEvent.GetType().Name);
        }
    }
}
