using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Sale;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Sale;

public class OrderItemCompletedEventConsumer(
    ILogger<OrderItemCompletedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<OrderItemCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderItemCompletedEvent> context)
    {
        var message = context.Message;

        var stats = await unitOfWork.AggCourseStatsRepository.GetByCourseIdAsync(message.CourseId);
        if (stats != null)
        {
            stats.TotalRevenue += message.LineTotal;
            stats.NetRevenue = stats.TotalRevenue - stats.TotalRefundAmount;
            await unitOfWork.AggCourseStatsRepository.UpdateAsync(stats.Id, stats);
        }

        var instructorRevenue = await unitOfWork.AggInstructorRevenueRepository.GetByInstructorIdAsync(message.InstructorId);
        if (instructorRevenue != null)
        {
            instructorRevenue.TotalRevenue += message.LineTotal;
            instructorRevenue.TotalPlatformFee += message.PlatformFeeAmount;
            instructorRevenue.TotalInstructorEarnings += message.InstructorEarnings;
            // Phase 2: credit immediately — AvailableBalance = total earned minus already paid out
            instructorRevenue.AvailableBalance = instructorRevenue.TotalInstructorEarnings - instructorRevenue.TotalPaidOut;
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalRevenue += message.LineTotal;
        overview.TotalPlatformFee += message.PlatformFeeAmount;
        overview.TotalInstructorEarnings += message.InstructorEarnings;

        var now = DateTime.UtcNow;
        var yearMonth = $"{now.Year:D4}-{now.Month:D2}";
        var monthly = await unitOfWork.AggSystemOverviewMonthlyRepository
            .GetOrCreateForMonthAsync(yearMonth, now.Year, now.Month);
        monthly.Revenue += message.LineTotal;
        monthly.PlatformProfit += message.PlatformFeeAmount;
        monthly.InstructorEarnings += message.InstructorEarnings;

        var dateKey = $"{now.Year:D4}-{now.Month:D2}-{now.Day:D2}";
        var daily = await unitOfWork.AggSystemOverviewDailyRepository
            .GetOrCreateForDateAsync(dateKey, now.Year, now.Month, now.Day);
        daily.Revenue += message.LineTotal;
        daily.PlatformProfit += message.PlatformFeeAmount;
        daily.InstructorEarnings += message.InstructorEarnings;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order item revenue updated in analytics: Course {CourseId}, Amount {Amount}",
            message.CourseId, message.LineTotal);
    }
}
