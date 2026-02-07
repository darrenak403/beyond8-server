using Beyond8.Sale.Application.Dtos.Payments;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Payments;

public class ProcessPaymentRequestValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId không được để trống");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Phương thức thanh toán không được để trống")
            .Must(method => method == "VNPay")
            .WithMessage("Phương thức thanh toán không hợp lệ. Hiện tại chỉ hỗ trợ VNPay");
    }
}