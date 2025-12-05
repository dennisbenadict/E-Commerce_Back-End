using FluentValidation;
using ProductService.Application.DTOs;

namespace ProductService.Application.Validators;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        Include(new CreateProductDtoValidator()); // same rules
    }
}
