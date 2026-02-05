using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Events.Assessment;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Assessment;

public class AssignmentDeletedEventConsumer(
    ILogger<AssignmentDeletedEventConsumer> logger,
    ISectionService sectionService) : IConsumer<AssignmentDeletedEvent>
{
    public async Task Consume(ConsumeContext<AssignmentDeletedEvent> context)
    {
        var assignmentId = context.Message.AssignmentId;

        var result = await sectionService.UnlinkSectionsByAssignmentIdAsync(assignmentId);

        if (result.IsSuccess)
            logger.LogInformation("Unlinked sections for deleted assignment: {AssignmentId}", assignmentId);
        else
            logger.LogWarning("Failed to unlink sections for assignment {AssignmentId}: {Message}", assignmentId, result.Message);
    }
}
