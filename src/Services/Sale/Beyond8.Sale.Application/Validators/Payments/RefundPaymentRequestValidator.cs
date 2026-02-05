using Beyond8.Sale.Application.Dtos.Payments;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Payments
{
    public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
    {
        public RefundPaymentRequestValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("OrderId không được để trống");

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0)
                .WithMessage("Số tiền hoàn trả phải lớn hơn 0");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Lý do hoàn trả không được để trống")
                .MaximumLength(500)
                .WithMessage("Lý do hoàn trả không được vượt quá 500 ký tự");
        }
    }
}