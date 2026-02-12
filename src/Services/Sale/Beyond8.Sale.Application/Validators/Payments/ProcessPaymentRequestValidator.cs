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
    }
}