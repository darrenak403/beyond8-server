using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Identity;

public class InstructorApprovalEventConsumer(
    ILogger<InstructorApprovalEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<InstructorApprovalEvent>
{
    public async Task Consume(ConsumeContext<InstructorApprovalEvent> context)
    {
        var message = context.Message;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalInstructors++;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Instructor approved in analytics: UserId {UserId}, TotalInstructors={Count}",
            message.UserId, overview.TotalInstructors);
    }
}
