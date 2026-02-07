using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Domain.Enums;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Orders;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái đơn hàng không hợp lệ");
    }
}