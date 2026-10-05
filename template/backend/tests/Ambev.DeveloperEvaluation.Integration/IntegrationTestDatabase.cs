using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public sealed class IntegrationTestDatabase : IAsyncLifetime
{
    public const string DefaultConnectionString =
        "Host=localhost;Port=5433;Database=developer_evaluation_test;Username=developer_test;Password=ev@luAt10n_test";

    public string ConnectionString =>
        Environment.GetEnvironmentVariable("INTEGRATION_TEST_CONNECTION_STRING")
        ?? DefaultConnectionString;

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public DefaultContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString, options => options.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM"))
            .Options;

        return new DefaultContext(options);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("Integration database", DisableParallelization = true)]
public sealed class IntegrationDatabaseCollection : ICollectionFixture<IntegrationTestDatabase>
{
}
