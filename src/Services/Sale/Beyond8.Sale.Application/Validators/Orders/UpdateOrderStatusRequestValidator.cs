using Beyond8.Sale.Application.Dtos.Orders;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Orders
{
    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Trạng thái đơn hàng không được để trống")
                .Must(status => new[] { "Pending", "Paid", "Cancelled", "Refunded" }.Contains(status))
                .WithMessage("Trạng thái đơn hàng không hợp lệ. Các giá trị hợp lệ: Pending, Paid, Cancelled, Refunded");
        }
    }
}