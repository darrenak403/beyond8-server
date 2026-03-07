using Beyond8.Sale.Application.Dtos.Carts;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Carts;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("CourseId không được để trống");
    }
}

public class CheckoutCartRequestValidator : AbstractValidator<CheckoutCartRequest>
{
    public CheckoutCartRequestValidator()
    {
        // ─── Validate selected items ───
        RuleForEach(x => x.SelectedItems)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.CourseId)
                    .NotEmpty()
                    .WithMessage("CourseId không được để trống");

                item.RuleFor(i => i.InstructorCouponCode)
                    .MaximumLength(50)
                    .WithMessage("Mã giảm giá instructor không được vượt quá 50 ký tự")
                    .When(i => !string.IsNullOrEmpty(i.InstructorCouponCode));
            })
            .When(x => x.SelectedItems.Any());

        // ─── Validate system coupon ───
        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .WithMessage("Mã giảm giá hệ thống không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        // ─── Validate notes ───
        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // ─── Validate no duplicate course IDs ───
        RuleFor(x => x.SelectedItems)
            .Must(items => items.Select(i => i.CourseId).Distinct().Count() == items.Count)
            .WithMessage("Không được chọn cùng một khóa học nhiều lần")
            .When(x => x.SelectedItems.Any());
    }
}

public class CheckoutItemRequestValidator : AbstractValidator<CheckoutItemRequest>
{
    public CheckoutItemRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("CourseId không được để trống");

        RuleFor(x => x.InstructorCouponCode)
            .MaximumLength(50)
            .WithMessage("Mã giảm giá instructor không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.InstructorCouponCode));
    }
}
