using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

/// <summary>
/// Validator for CreateUserCommand that defines validation rules for user creation command.
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateUserCommandValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - Email: Must be in valid format (using EmailValidator)
    /// - Username: Required, must be between 3 and 50 characters
    /// - Password: Must meet security requirements (using PasswordValidator)
    /// - Phone: Must match international format (+X XXXXXXXXXX)
    /// - Status: Cannot be set to Unknown
    /// - Role: Cannot be set to None
    /// </remarks>
    public CreateUserCommandValidator()
    {
        RuleFor(user => user.Email).SetValidator(new EmailValidator());
        RuleFor(user => user.Username).NotEmpty().Length(3, 50);
        RuleFor(user => user.Name).NotNull().SetValidator(new UserNameModelValidator());
        RuleFor(user => user.Address).NotNull().SetValidator(new UserAddressModelValidator());
        RuleFor(user => user.Password).SetValidator(new PasswordValidator());
        RuleFor(user => user.Phone).Matches(@"^\+?[1-9]\d{1,14}$");
        RuleFor(user => user.Status).NotEqual(UserStatus.Unknown);
        RuleFor(user => user.Role).NotEqual(UserRole.None);
    }
}

public sealed class UserNameModelValidator : AbstractValidator<UserNameModel>
{
    public UserNameModelValidator()
    {
        RuleFor(name => name.Firstname).NotEmpty().MaximumLength(100);
        RuleFor(name => name.Lastname).NotEmpty().MaximumLength(100);
    }
}

public sealed class UserAddressModelValidator : AbstractValidator<UserAddressModel>
{
    public UserAddressModelValidator()
    {
        RuleFor(address => address.City).NotEmpty().MaximumLength(100);
        RuleFor(address => address.Street).NotEmpty().MaximumLength(150);
        RuleFor(address => address.Number).GreaterThan(0);
        RuleFor(address => address.Zipcode).NotEmpty().MaximumLength(20);
        RuleFor(address => address.Geolocation)
            .NotNull()
            .SetValidator(new UserGeolocationModelValidator());
    }
}

public sealed class UserGeolocationModelValidator : AbstractValidator<UserGeolocationModel>
{
    public UserGeolocationModelValidator()
    {
        RuleFor(geolocation => geolocation.Lat).NotEmpty().MaximumLength(50);
        RuleFor(geolocation => geolocation.Long).NotEmpty().MaximumLength(50);
    }
}
