using FluentValidation;
using UserService.Application.DTOs;

namespace UserService.Application.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
	public UpdateProfileValidator()
	{
		RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Email).NotEmpty().EmailAddress();
		RuleFor(x => x.Phone).NotEmpty().MaximumLength(20).Matches(@"^[0-9]{10}$");
	}
}
