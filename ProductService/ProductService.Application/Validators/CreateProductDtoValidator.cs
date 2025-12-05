using FluentValidation;
using ProductService.Application.DTOs;

namespace ProductService.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be valid.");

        RuleFor(x => x.Sizes)
            .NotNull()
            .Must(s => s.Count > 0).WithMessage("At least one size is required.");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("Image URL cannot be empty.");

        RuleFor(x => x.Gender)
            .Must(g => new[] { "Men", "Women", "Unisex" }.Contains(g))
            .WithMessage("Gender must be Men, Women, or Unisex.");
    }
}
