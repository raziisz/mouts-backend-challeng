using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0.10)]
    [InlineData(9, 0.10)]
    [InlineData(10, 0.20)]
    [InlineData(20, 0.20)]
    public void Discount_should_follow_quantity_tiers(int quantity, decimal expectedRate)
    {
        DiscountPolicy.GetRate(quantity).Should().Be(expectedRate);
    }

    [Fact]
    public void Quantity_above_twenty_should_be_rejected()
    {
        var sale = new Sale();

        var action = () => sale.AddItem(1, "Product", 10m, 21);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void Item_total_should_include_discount()
    {
        var sale = new Sale();
        sale.AddItem(1, "Product", 10m, 10);

        sale.TotalAmount.Should().Be(80m);
        sale.Items.Single().DiscountAmount.Should().Be(20m);
    }
}
