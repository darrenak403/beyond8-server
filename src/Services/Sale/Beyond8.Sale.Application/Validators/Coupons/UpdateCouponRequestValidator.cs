using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Domain.Enums;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Coupons;

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Loại coupon không hợp lệ. Các giá trị hợp lệ: Percentage, FixedAmount")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Giá trị coupon phải lớn hơn 0")
            .When(x => x.Value.HasValue);

        RuleFor(x => x.Value)
            .InclusiveBetween(1, 100)
            .WithMessage("Giá trị coupon phần trăm phải từ 1 đến 100")
            .When(x => x.Value.HasValue && x.Type == CouponType.Percentage);

        RuleFor(x => x.MinOrderAmount)
            .GreaterThan(0)
            .WithMessage("Giá trị đơn hàng tối thiểu phải lớn hơn 0")
            .When(x => x.MinOrderAmount.HasValue);

        RuleFor(x => x.MaxDiscountAmount)
            .GreaterThan(0)
            .WithMessage("Giá trị giảm giá tối đa phải lớn hơn 0")
            .When(x => x.MaxDiscountAmount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("Số lần sử dụng tối đa phải lớn hơn 0")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.UsagePerUser)
            .GreaterThan(0)
            .WithMessage("Số lần sử dụng tối đa mỗi người dùng phải lớn hơn 0")
            .When(x => x.UsagePerUser.HasValue);

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom!.Value)
            .WithMessage("Ngày hết hạn phải sau ngày bắt đầu")
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}