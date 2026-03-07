using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Identity;

public class InstructorHiddenEventConsumer(
    ILogger<InstructorHiddenEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<InstructorHiddenEvent>
{
    public async Task Consume(ConsumeContext<InstructorHiddenEvent> context)
    {
        var message = context.Message;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        if (overview.TotalInstructors > 0)
            overview.TotalInstructors--;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Instructor hidden in analytics: UserId {UserId}, TotalInstructors={Count}",
            message.UserId, overview.TotalInstructors);
    }
}
