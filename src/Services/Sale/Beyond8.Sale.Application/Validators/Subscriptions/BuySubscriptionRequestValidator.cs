using FluentValidation;
using Beyond8.Sale.Application.Dtos.Subscriptions;

namespace Beyond8.Sale.Application.Validators.Subscriptions
{
    public class BuySubscriptionRequestValidator : AbstractValidator<BuySubscriptionRequest>
    {
        public BuySubscriptionRequestValidator()
        {
            RuleFor(x => x.PlanCode)
                .NotEmpty().WithMessage("Mã gói subscription không được để trống")
                .MaximumLength(64).WithMessage("Mã gói subscription không được dài hơn 64 ký tự");
        }
    }
}
