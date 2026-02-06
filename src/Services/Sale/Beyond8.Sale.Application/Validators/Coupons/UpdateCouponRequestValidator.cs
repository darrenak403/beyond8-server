using Beyond8.Sale.Application.Dtos.Coupons;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Coupons
{
    public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
    {
        public UpdateCouponRequestValidator()
        {
            RuleFor(x => x.Type)
                .Must(type => string.IsNullOrEmpty(type) || new[] { "Percentage", "Fixed" }.Contains(type))
                .WithMessage("Loại coupon không hợp lệ. Các giá trị hợp lệ: Percentage, Fixed")
                .When(x => !string.IsNullOrEmpty(x.Type));

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Giá trị coupon phải lớn hơn 0")
                .When(x => x.Value.HasValue && x.Type == "Fixed")
                .InclusiveBetween(1, 100)
                .When(x => x.Value.HasValue && x.Type == "Percentage")
                .WithMessage("Giá trị coupon phần trăm phải từ 1 đến 100");

            RuleFor(x => x.MinOrderAmount)
                .GreaterThan(0)
                .WithMessage("Giá trị đơn hàng tối thiểu phải lớn hơn 0")
                .When(x => x.MinOrderAmount.HasValue);

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0)
                .WithMessage("Số lần sử dụng tối đa phải lớn hơn 0")
                .When(x => x.UsageLimit.HasValue);

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Thời gian hết hạn phải trong tương lai")
                .When(x => x.ExpiresAt.HasValue);
        }
    }
}