using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(product => product.Title).NotEmpty().MaximumLength(200);
        RuleFor(product => product.Price).GreaterThan(0);
        RuleFor(product => product.Description).MaximumLength(2000);
        RuleFor(product => product.Category).NotEmpty().MaximumLength(100);
        RuleFor(product => product.Image).MaximumLength(500);
        RuleFor(product => product.RatingRate).InclusiveBetween(0, 5);
        RuleFor(product => product.RatingCount).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(product => product.Id).GreaterThan(0);
        RuleFor(product => product.Title).NotEmpty().MaximumLength(200);
        RuleFor(product => product.Price).GreaterThan(0);
        RuleFor(product => product.Description).MaximumLength(2000);
        RuleFor(product => product.Category).NotEmpty().MaximumLength(100);
        RuleFor(product => product.Image).MaximumLength(500);
        RuleFor(product => product.RatingRate).InclusiveBetween(0, 5);
        RuleFor(product => product.RatingCount).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.Size).InclusiveBetween(1, 100);
        RuleFor(query => query.Price).GreaterThan(0).When(query => query.Price.HasValue);
        RuleFor(query => query.MinPrice).GreaterThanOrEqualTo(0).When(query => query.MinPrice.HasValue);
        RuleFor(query => query.MaxPrice).GreaterThanOrEqualTo(0).When(query => query.MaxPrice.HasValue);
        RuleFor(query => query.MinPrice)
            .LessThanOrEqualTo(query => query.MaxPrice!.Value)
            .When(query => query.MinPrice.HasValue && query.MaxPrice.HasValue);
    }
}

public sealed class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator() => RuleFor(query => query.Id).GreaterThan(0);
}

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(command => command.Id).GreaterThan(0);
}
