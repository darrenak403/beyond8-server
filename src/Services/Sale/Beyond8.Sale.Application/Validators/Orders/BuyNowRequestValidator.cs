using Beyond8.Sale.Application.Dtos.Orders;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Orders;

public class BuyNowRequestValidator : AbstractValidator<BuyNowRequest>
{
    public BuyNowRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("CourseId không được để trống");

        RuleFor(x => x.InstructorCouponCode)
            .MaximumLength(50)
            .WithMessage("Mã giảm giá instructor không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.InstructorCouponCode));

        RuleFor(x => x.CouponCode)
            .MaximumLength(50)
            .WithMessage("Mã giảm giá hệ thống không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.CouponCode));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
