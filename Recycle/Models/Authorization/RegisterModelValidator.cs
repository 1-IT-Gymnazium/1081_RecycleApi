using FluentValidation;

namespace Recycle.Api.Models.Authorization;

public class RegisterModelValidator : AbstractValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Matches("^[a-zA-Z]+$").WithMessage("First name can only contain letters.")
            .MaximumLength(20).WithMessage("UserName must be at most 20 characters.");
        ;
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Matches("^[a-zA-Z]+$").WithMessage("Last name can only contain letters.")
            .MaximumLength(20).WithMessage("UserName must be at most 20 characters.");
        ;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("UserName can only contain letters and digits.")
            .MinimumLength(3).WithMessage("UserName must be at least 3 characters.")
            .MaximumLength(20).WithMessage("UserName must be at most 20 characters.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
