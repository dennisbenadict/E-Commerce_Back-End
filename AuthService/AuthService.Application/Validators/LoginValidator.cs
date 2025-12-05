using AuthService.Application.DTOs;
using FluentValidation;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password required");
    }
}
