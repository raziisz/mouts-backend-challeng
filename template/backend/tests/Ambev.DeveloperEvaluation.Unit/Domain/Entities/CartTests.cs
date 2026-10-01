using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class CartTests
{
    [Fact]
    public void Adding_same_product_should_accumulate_quantity()
    {
        var cart = new Cart();

        cart.AddItem(1, 2);
        cart.AddItem(1, 3);

        cart.Items.Single().Quantity.Should().Be(5);
    }
}
