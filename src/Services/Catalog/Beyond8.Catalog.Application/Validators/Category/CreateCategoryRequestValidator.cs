using Beyond8.Catalog.Application.Dtos.Categories;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Category;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục không được để trống")
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.IsRoot)
            .Equal(false)
            .WithMessage("Danh mục con không thể là danh mục gốc")
            .When(x => x.ParentId.HasValue);

        RuleFor(x => x.IsRoot)
            .Equal(true)
            .WithMessage("Danh mục gốc phải có IsRoot = true")
            .When(x => !x.ParentId.HasValue);
    }
}
