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
        RuleForEach(x => x.SelectedCourseIds)
            .NotEmpty()
            .WithMessage("CourseId không được để trống")
            .When(x => x.SelectedCourseIds.Any());

        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .WithMessage("Mã giảm giá không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
