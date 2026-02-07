using Beyond8.Learning.Application.Dtos.Progress;
using FluentValidation;

namespace Beyond8.Learning.Application.Validators.Progress;

public class LessonProgressHeartbeatRequestValidator : AbstractValidator<LessonProgressHeartbeatRequest>
{
    public LessonProgressHeartbeatRequestValidator()
    {
        RuleFor(x => x.LastPositionSeconds)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LastPositionSeconds.HasValue)
            .WithMessage("Vị trí phát phải lớn hơn hoặc bằng 0.");
    }
}
