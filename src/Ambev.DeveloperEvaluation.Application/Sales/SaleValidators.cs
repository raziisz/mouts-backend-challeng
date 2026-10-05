using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public sealed class SaleItemInputModelValidator : AbstractValidator<SaleItemInputModel>
{
    public SaleItemInputModelValidator()
    {
        RuleFor(item => item.ProductId).GreaterThan(0);
        RuleFor(item => item.ProductDescription).NotEmpty().MaximumLength(200);
        RuleFor(item => item.UnitPrice).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(sale => sale.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(sale => sale.Date).NotEqual(default(DateTime));
        RuleFor(sale => sale.CustomerId).GreaterThan(0);
        RuleFor(sale => sale.CustomerDescription).NotEmpty().MaximumLength(200);
        RuleFor(sale => sale.BranchId).GreaterThan(0);
        RuleFor(sale => sale.BranchDescription).NotEmpty().MaximumLength(200);
        RuleFor(sale => sale.Products).NotNull().NotEmpty();
        RuleForEach(sale => sale.Products).SetValidator(new SaleItemInputModelValidator());
    }
}

public sealed class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(sale => sale.Id).GreaterThan(0);
        RuleFor(sale => sale.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(sale => sale.Date).NotEqual(default(DateTime));
        RuleFor(sale => sale.CustomerId).GreaterThan(0);
        RuleFor(sale => sale.CustomerDescription).NotEmpty().MaximumLength(200);
        RuleFor(sale => sale.BranchId).GreaterThan(0);
        RuleFor(sale => sale.BranchDescription).NotEmpty().MaximumLength(200);
        RuleFor(sale => sale.Products).NotNull().NotEmpty();
        RuleForEach(sale => sale.Products).SetValidator(new SaleItemInputModelValidator());
    }
}

public sealed class GetSalesQueryValidator : AbstractValidator<GetSalesQuery>
{
    public GetSalesQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.Size).InclusiveBetween(1, 100);
        RuleFor(query => query.Status)
            .Must(status => Enum.TryParse<SaleStatus>(status, true, out _))
            .When(query => !string.IsNullOrWhiteSpace(query.Status));
        RuleFor(query => query.MinDate)
            .LessThanOrEqualTo(query => query.MaxDate!.Value)
            .When(query => query.MinDate.HasValue && query.MaxDate.HasValue);
    }
}

public sealed class GetSaleQueryValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleQueryValidator() => RuleFor(query => query.Id).GreaterThan(0);
}

public sealed class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleCommandValidator() => RuleFor(command => command.Id).GreaterThan(0);
}

public sealed class CancelSaleItemCommandValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemCommandValidator()
    {
        RuleFor(command => command.SaleId).GreaterThan(0);
        RuleFor(command => command.ItemId).GreaterThan(0);
    }
}
