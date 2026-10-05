using FluentValidation;
using System.Text.RegularExpressions;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(email => email)
            .NotEmpty()
            .WithMessage("The email address cannot be empty.")
            .MaximumLength(100)
            .WithMessage("The email address cannot be longer than 100 characters.")
            .Must(BeValidEmail)
            .WithMessage("The provided email address is not valid.");
    }

    private bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Accepts regular domains and local development hosts such as admin@localhost.
        var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$");
        return regex.IsMatch(email);
    }
}
