using Beyond8.Sale.Application.Dtos.Orders;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators;

/// <summary>
/// Validator for PreviewOrderRequest.
/// </summary>
public class PreviewOrderRequestValidator : AbstractValidator<PreviewOrderRequest>
{
    public PreviewOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Danh sách khóa học không được để trống")
            .Must(items => items.Count <= 50).WithMessage("Không được thêm quá 50 khóa học trong một đơn hàng");

        RuleForEach(x => x.Items)
            .SetValidator(new PreviewOrderItemRequestValidator());

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Mã coupon không được quá 50 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}

/// <summary>
/// Validator for individual preview order items.
/// </summary>
public class PreviewOrderItemRequestValidator : AbstractValidator<PreviewOrderItemRequest>
{
    public PreviewOrderItemRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("ID khóa học không được để trống");

        RuleFor(x => x.InstructorCouponCode)
            .MaximumLength(50).WithMessage("Mã coupon instructor không được quá 50 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.InstructorCouponCode));
    }
}