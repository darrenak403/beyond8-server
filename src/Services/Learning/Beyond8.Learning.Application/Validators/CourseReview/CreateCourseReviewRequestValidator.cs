using Beyond8.Learning.Application.Dtos.CourseReview;
using FluentValidation;

namespace Beyond8.Learning.Application.Validators.CourseReview;

public class CreateCourseReviewRequestValidator : AbstractValidator<CreateCourseReviewRequest>
{
    private const int MinRating = 1;
    private const int MaxRating = 5;
    private const int MaxReviewLength = 2000;

    public CreateCourseReviewRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Mã khóa học không được để trống.");

        RuleFor(x => x.EnrollmentId)
            .NotEmpty()
            .WithMessage("Mã đăng ký không được để trống.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(MinRating, MaxRating)
            .WithMessage($"Điểm đánh giá phải từ {MinRating} đến {MaxRating}.");

        RuleFor(x => x.Review)
            .MaximumLength(MaxReviewLength)
            .When(x => !string.IsNullOrEmpty(x.Review))
            .WithMessage($"Nội dung đánh giá không được quá {MaxReviewLength} ký tự.");

        RuleFor(x => x.ContentQuality)
            .InclusiveBetween(MinRating, MaxRating)
            .When(x => x.ContentQuality.HasValue)
            .WithMessage($"Điểm chất lượng nội dung phải từ {MinRating} đến {MaxRating}.");

        RuleFor(x => x.InstructorQuality)
            .InclusiveBetween(MinRating, MaxRating)
            .When(x => x.InstructorQuality.HasValue)
            .WithMessage($"Điểm chất lượng giảng viên phải từ {MinRating} đến {MaxRating}.");

        RuleFor(x => x.ValueForMoney)
            .InclusiveBetween(MinRating, MaxRating)
            .When(x => x.ValueForMoney.HasValue)
            .WithMessage($"Điểm giá trị phải từ {MinRating} đến {MaxRating}.");
    }
}
