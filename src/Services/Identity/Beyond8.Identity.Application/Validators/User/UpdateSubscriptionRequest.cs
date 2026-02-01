using Beyond8.Identity.Application.Dtos.Users;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.User;

public class UpdateUsageQuotaRequestValidator : AbstractValidator<UpdateUsageQuotaRequest>
{
    public UpdateUsageQuotaRequestValidator()
    {
        RuleFor(x => x.NumberOfRequests).GreaterThan(0);
        RuleFor(x => x.NumberOfRequests).LessThanOrEqualTo(100);
    }
}
