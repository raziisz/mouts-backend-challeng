using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
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

    [Fact]
    public void Cancelling_sale_should_cancel_all_items_and_zero_total()
    {
        var sale = new Sale();
        sale.AddItem(1, "Product 1", 10m, 4);
        sale.AddItem(2, "Product 2", 20m, 2);

        sale.Cancel();

        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Items.Should().OnlyContain(item => item.IsCancelled);
        sale.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void Cancelling_item_should_preserve_other_items_total()
    {
        var sale = new Sale();
        sale.AddItem(1, "Product 1", 10m, 4);
        sale.AddItem(2, "Product 2", 20m, 2);
        sale.Items[0].Id = 10;
        sale.Items[1].Id = 11;

        sale.CancelItem(10);

        sale.Items[0].IsCancelled.Should().BeTrue();
        sale.Items[1].IsCancelled.Should().BeFalse();
        sale.TotalAmount.Should().Be(40m);
    }

    [Fact]
    public void Replacing_items_on_cancelled_sale_should_keep_items_cancelled()
    {
        var sale = new Sale();
        sale.AddItem(1, "Product", 10m, 2);
        sale.Cancel();

        sale.ReplaceItems([new SaleItemInput(2, "Replacement", 25m, 4)]);

        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Items.Should().OnlyContain(item => item.IsCancelled);
        sale.TotalAmount.Should().Be(0m);
    }
}
