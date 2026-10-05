using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class DefaultContextModelTests
{
    [Fact]
    public void Model_should_include_all_transactional_aggregates_with_integer_ids()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(IntegrationTestDatabase.DefaultConnectionString)
            .Options;

        using var context = new DefaultContext(options);

        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.User"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Product"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Cart"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Sale"));
        Assert.Equal(typeof(int), context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Sale")!.FindProperty("Id")!.ClrType);
    }
}

[Collection("Integration database")]
public sealed class DefaultContextDatabaseTests(IntegrationTestDatabase database)
{
    [Fact]
    public async Task Database_should_be_available_and_have_all_migrations_applied()
    {
        await using var context = database.CreateContext();

        Assert.True(await context.Database.CanConnectAsync());

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20261005044213_EnforceUniqueUserIdentifiers", appliedMigrations);
    }
}
