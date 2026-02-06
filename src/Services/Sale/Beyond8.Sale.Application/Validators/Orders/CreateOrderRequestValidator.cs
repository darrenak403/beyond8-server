using Beyond8.Sale.Application.Dtos.Orders;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Orders;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId không được để trống");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Danh sách sản phẩm không được để trống")
            .Must(items => items.Count > 0)
            .WithMessage("Phải có ít nhất một sản phẩm trong đơn hàng");

        RuleForEach(x => x.Items)
            .ChildRules(items =>
            {
                items.RuleFor(item => item.CourseId)
                    .NotEmpty()
                    .WithMessage("CourseId không được để trống");

                // Per BR-04: Free courses have Price = 0
                items.RuleFor(item => item.Price)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Giá sản phẩm không được âm");
            });

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