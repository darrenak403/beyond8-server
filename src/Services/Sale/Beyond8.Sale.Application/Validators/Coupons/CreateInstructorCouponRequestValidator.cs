using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Domain.Enums;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Coupons;

public class CreateInstructorCouponRequestValidator : AbstractValidator<CreateInstructorCouponRequest>
{
    public CreateInstructorCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Mã coupon không được để trống")
            .MaximumLength(50)
            .WithMessage("Mã coupon không được vượt quá 50 ký tự")
            .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Mã coupon chỉ được chứa chữ hoa và số");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Loại coupon không hợp lệ. Các giá trị hợp lệ: Percentage, FixedAmount");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Giá trị coupon phải lớn hơn 0");

        RuleFor(x => x.Value)
            .InclusiveBetween(1, 100)
            .WithMessage("Giá trị coupon phần trăm phải từ 1 đến 100")
            .When(x => x.Type == CouponType.Percentage);

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

        RuleFor(x => x.ApplicableCourseId)
            .NotEmpty()
            .WithMessage("Phải chọn khóa học áp dụng coupon");

        RuleFor(x => x.ValidFrom)
            .NotEmpty()
            .WithMessage("Ngày bắt đầu hiệu lực không được để trống");

        RuleFor(x => x.ValidTo)
            .NotEmpty()
            .WithMessage("Ngày hết hạn không được để trống")
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("Ngày hết hạn phải sau ngày bắt đầu");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}