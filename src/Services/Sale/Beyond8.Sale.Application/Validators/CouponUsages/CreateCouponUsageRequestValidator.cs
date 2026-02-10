using Beyond8.Sale.Application.Dtos.CouponUsages;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.CouponUsages;

public class CreateCouponUsageRequestValidator : AbstractValidator<CreateCouponUsageRequest>
{
    public CreateCouponUsageRequestValidator()
    {
        RuleFor(x => x.CouponId)
            .NotEmpty().WithMessage("CouponId không được để trống");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được để trống");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId không được để trống");

        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("CouponCode không được để trống")
            .MaximumLength(50).WithMessage("CouponCode không được vượt quá 50 ký tự");

        RuleFor(x => x.CouponType)
            .NotEmpty().WithMessage("CouponType không được để trống")
            .Must(type => type == "Percentage" || type == "FixedAmount")
            .WithMessage("CouponType phải là 'Percentage' hoặc 'FixedAmount'");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("DiscountValue phải lớn hơn 0");

        RuleFor(x => x.DiscountApplied)
            .GreaterThanOrEqualTo(0).WithMessage("DiscountApplied phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.OrderSubtotal)
            .GreaterThan(0).WithMessage("OrderSubtotal phải lớn hơn 0");

        // Business rule: DiscountApplied should not exceed OrderSubtotal
        RuleFor(x => x.DiscountApplied)
            .LessThanOrEqualTo(x => x.OrderSubtotal)
            .WithMessage("DiscountApplied không được vượt quá OrderSubtotal");
    }
}