using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Sales;
using FluentValidation;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public sealed class CatalogValidatorTests
{
    [Fact]
    public void Product_validator_should_reject_invalid_business_input()
    {
        var command = new CreateProductCommand("", 0, new string('x', 2001), "", "", 6, -1);

        var result = new CreateProductCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Title));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Price));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.RatingRate));
    }

    [Fact]
    public void Cart_validator_should_reject_invalid_items_but_allow_empty_cart()
    {
        var validEmptyCart = new CreateCartCommand(10, DateTime.UtcNow, []);
        var invalidCart = new CreateCartCommand(10, DateTime.UtcNow, [new CartItemResult { ProductId = 0, Quantity = 0 }]);

        Assert.True(new CreateCartCommandValidator().Validate(validEmptyCart).IsValid);
        Assert.False(new CreateCartCommandValidator().Validate(invalidCart).IsValid);
    }

    [Fact]
    public void Sale_validator_should_reject_invalid_input_and_allow_domain_limit_to_handle_over_20()
    {
        var invalid = new CreateSaleCommand(
            "",
            default,
            0,
            "",
            0,
            "",
            [new SaleItemInputModel { ProductId = 0, ProductDescription = "", UnitPrice = 0, Quantity = 0 }]);
        var quantityLimitCandidate = invalid with
        {
            SaleNumber = "SALE-1",
            Date = DateTime.UtcNow,
            CustomerId = 1,
            CustomerDescription = "Customer",
            BranchId = 1,
            BranchDescription = "Branch",
            Products = [new SaleItemInputModel { ProductId = 1, ProductDescription = "Product", UnitPrice = 10, Quantity = 21 }]
        };

        Assert.False(new CreateSaleCommandValidator().Validate(invalid).IsValid);
        Assert.True(new CreateSaleCommandValidator().Validate(quantityLimitCandidate).IsValid);
    }
}
