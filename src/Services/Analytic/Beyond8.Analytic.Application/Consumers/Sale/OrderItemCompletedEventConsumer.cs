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
            instructorRevenue.PendingBalance = instructorRevenue.TotalInstructorEarnings - instructorRevenue.TotalPaidOut;
            await unitOfWork.AggInstructorRevenueRepository.UpdateAsync(instructorRevenue.Id, instructorRevenue);
        }

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalRevenue += message.LineTotal;
        overview.TotalPlatformFee += message.PlatformFeeAmount;
        overview.TotalInstructorEarnings += message.InstructorEarnings;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order item revenue updated in analytics: Course {CourseId}, Amount {Amount}",
            message.CourseId, message.LineTotal);
    }
}
