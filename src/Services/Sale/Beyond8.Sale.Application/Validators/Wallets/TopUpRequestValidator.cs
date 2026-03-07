using Beyond8.Sale.Application.Dtos.Wallets;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators.Wallets;

public class TopUpRequestValidator : AbstractValidator<TopUpRequest>
{
    public TopUpRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(10000)
            .WithMessage("Số tiền nạp tối thiểu là 10.000 VND");
    }
}
