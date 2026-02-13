using Beyond8.Sale.Application.Dtos.Orders;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators;

/// <summary>
/// Validator for PreviewBuyNowRequest.
/// </summary>
public class PreviewBuyNowRequestValidator : AbstractValidator<PreviewBuyNowRequest>
{
    public PreviewBuyNowRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("ID khóa học không được để trống");

        RuleFor(x => x.InstructorCouponCode)
            .MaximumLength(50).WithMessage("Mã coupon instructor không được quá 50 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.InstructorCouponCode));

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Mã coupon hệ thống không được quá 50 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}