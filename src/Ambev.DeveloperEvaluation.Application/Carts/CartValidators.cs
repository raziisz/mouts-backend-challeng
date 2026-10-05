using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts;

public sealed class CartItemResultValidator : AbstractValidator<CartItemResult>
{
    public CartItemResultValidator()
    {
        RuleFor(item => item.ProductId).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}

public sealed class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartCommandValidator()
    {
        RuleFor(cart => cart.UserId).GreaterThan(0);
        RuleFor(cart => cart.Date).NotEqual(default(DateTime));
        RuleFor(cart => cart.Products).NotNull();
        RuleForEach(cart => cart.Products).SetValidator(new CartItemResultValidator());
    }
}

public sealed class UpdateCartCommandValidator : AbstractValidator<UpdateCartCommand>
{
    public UpdateCartCommandValidator()
    {
        RuleFor(cart => cart.Id).GreaterThan(0);
        RuleFor(cart => cart.UserId).GreaterThan(0);
        RuleFor(cart => cart.Date).NotEqual(default(DateTime));
        RuleFor(cart => cart.Products).NotNull();
        RuleForEach(cart => cart.Products).SetValidator(new CartItemResultValidator());
    }
}

public sealed class GetCartsQueryValidator : AbstractValidator<GetCartsQuery>
{
    public GetCartsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.Size).InclusiveBetween(1, 100);
        RuleFor(query => query.UserId).GreaterThan(0).When(query => query.UserId.HasValue);
        RuleFor(query => query.MinDate)
            .LessThanOrEqualTo(query => query.MaxDate!.Value)
            .When(query => query.MinDate.HasValue && query.MaxDate.HasValue);
    }
}

public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator() => RuleFor(query => query.Id).GreaterThan(0);
}

public sealed class DeleteCartCommandValidator : AbstractValidator<DeleteCartCommand>
{
    public DeleteCartCommandValidator() => RuleFor(command => command.Id).GreaterThan(0);
}
