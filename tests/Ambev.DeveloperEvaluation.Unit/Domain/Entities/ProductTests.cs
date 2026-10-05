using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Valid_product_should_pass_business_rules()
    {
        var product = new Product { Title = "Product", Price = 10m, Category = "Category", RatingRate = 4.5m };

        product.Invoking(x => x.ValidateBusinessRules()).Should().NotThrow();
    }

    [Fact]
    public void Non_positive_price_should_fail_business_rules()
    {
        var product = new Product { Title = "Product", Price = 0m, Category = "Category" };

        product.Invoking(x => x.ValidateBusinessRules()).Should().Throw<DomainException>();
    }
}
