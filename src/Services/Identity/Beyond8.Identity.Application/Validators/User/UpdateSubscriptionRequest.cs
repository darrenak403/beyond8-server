using System;
using Beyond8.Identity.Application.Dtos.Users;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.User;

public class UpdateSubscriptionRequestValidator : AbstractValidator<UpdateSubscriptionRequest>
{
    public UpdateSubscriptionRequestValidator()
    {
        RuleFor(x => x.NumberOfRequests).GreaterThan(0);
        RuleFor(x => x.NumberOfRequests).LessThanOrEqualTo(100);
    }
}
