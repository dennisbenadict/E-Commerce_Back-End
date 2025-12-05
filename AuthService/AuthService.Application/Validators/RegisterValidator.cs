using AuthService.Application.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Must(BeValidEmail).WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^[0-9]{10}$").WithMessage("Phone must be 10 digits");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Matches(@"^(?=.*[A-Z])(?=.*\d).{8,}$")
            .WithMessage("Password must be 8+ chars, include 1 uppercase & 1 number");
    }

    private bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var pattern = @"^(?!.*[-.]{2})[a-zA-Z0-9]+([._%+-]?[a-zA-Z0-9]+)*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}
