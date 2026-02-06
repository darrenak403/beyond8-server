using Beyond8.Sale.Application.Dtos.Coupons;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Coupons
{
    public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
    {
        public CreateCouponRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Mã coupon không được để trống")
                .MaximumLength(50)
                .WithMessage("Mã coupon không được vượt quá 50 ký tự")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Mã coupon chỉ được chứa chữ hoa và số");

            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage("Loại coupon không được để trống")
                .Must(type => new[] { "Percentage", "Fixed" }.Contains(type))
                .WithMessage("Loại coupon không hợp lệ. Các giá trị hợp lệ: Percentage, Fixed");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Giá trị coupon phải lớn hơn 0")
                .When(x => x.Type == "Fixed")
                .WithMessage("Giá trị coupon cố định phải lớn hơn 0")
                .InclusiveBetween(1, 100)
                .When(x => x.Type == "Percentage")
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