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
            .UseNpgsql("Host=localhost;Database=developer_evaluation;Username=developer;Password=ev@luAt10n")
            .Options;

        using var context = new DefaultContext(options);

        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.User"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Product"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Cart"));
        Assert.NotNull(context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Sale"));
        Assert.Equal(typeof(int), context.Model.FindEntityType("Ambev.DeveloperEvaluation.Domain.Entities.Sale")!.FindProperty("Id")!.ClrType);
    }
}
