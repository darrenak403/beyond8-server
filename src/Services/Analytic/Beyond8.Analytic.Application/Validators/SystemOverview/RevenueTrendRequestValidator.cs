using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Domain.Enums;
using FluentValidation;

namespace Beyond8.Analytic.Application.Validators.SystemOverview;

public class RevenueTrendRequestValidator : AbstractValidator<RevenueTrendRequest>
{
    public RevenueTrendRequestValidator()
    {
        RuleFor(x => x.GroupBy)
            .IsInEnum().WithMessage("Loại nhóm không hợp lệ");

        // Year is required for all modes except Custom
        When(x => x.GroupBy != GroupByPeriod.Custom, () =>
        {
            RuleFor(x => x.Year)
                .NotNull().WithMessage("Năm không được để trống")
                .GreaterThan(2000).WithMessage("Năm không hợp lệ (phải lớn hơn 2000)")
                .LessThanOrEqualTo(DateTime.UtcNow.Year + 1)
                .WithMessage("Năm không được vượt quá năm hiện tại");
        });

        When(x => x.GroupBy == GroupByPeriod.Quarter, () =>
        {
            RuleFor(x => x.Quarter)
                .NotNull().WithMessage("Quý không được để trống khi nhóm theo quý")
                .GreaterThanOrEqualTo(1).WithMessage("Quý phải từ 1 đến 4")
                .LessThanOrEqualTo(4).WithMessage("Quý phải từ 1 đến 4");
        });

        When(x => x.GroupBy == GroupByPeriod.Month, () =>
        {
            RuleFor(x => x.Month)
                .NotNull().WithMessage("Tháng không được để trống khi nhóm theo tháng")
                .GreaterThanOrEqualTo(1).WithMessage("Tháng phải từ 1 đến 12")
                .LessThanOrEqualTo(12).WithMessage("Tháng phải từ 1 đến 12");
        });

        When(x => x.GroupBy == GroupByPeriod.Custom, () =>
        {
            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("Ngày bắt đầu không được để trống khi nhóm theo khoảng thời gian");

            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("Ngày kết thúc không được để trống khi nhóm theo khoảng thời gian");

            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x.EndDate)
                    .GreaterThanOrEqualTo(x => x.StartDate!.Value)
                    .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu");
            });
        });
    }
}
