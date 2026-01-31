using Beyond8.Integration.Application.Dtos.AiIntegration.Profile;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration;

public class ProfileReviewRequestValidator : AbstractValidator<ProfileReviewRequest>
{
    public ProfileReviewRequestValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(2000).WithMessage("Bio không được vượt quá 2000 ký tự");

        RuleFor(x => x.Headline)
            .MaximumLength(200).WithMessage("Headline không được vượt quá 200 ký tự");

        RuleFor(x => x.ExpertiseAreas)
            .NotNull().WithMessage("ExpertiseAreas không được null");

        RuleFor(x => x.Education)
            .NotNull().WithMessage("Education không được null");

        RuleFor(x => x.WorkExperience)
            .NotNull().WithMessage("WorkExperience không được null");

        RuleFor(x => x.Certificates)
            .NotNull().WithMessage("Certificates không được null");

        RuleFor(x => x.TeachingLanguages)
            .NotNull().WithMessage("TeachingLanguages không được null");
    }
}
