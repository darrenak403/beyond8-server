using System;
using Beyond8.Catalog.Application.Dtos.Categories;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Category;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(255).WithMessage("Tên danh mục không được vượt quá 255 ký tự");
    }
}
