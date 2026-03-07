using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Identity;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Identity;

public class UserRegisteredEventConsumer(
    ILogger<UserRegisteredEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        var overview = await unitOfWork.AggSystemOverviewRepository.GetOrCreateCurrentAsync();
        overview.TotalUsers++;
        overview.NewUsersToday++;

        var roleUpper = message.RoleCode.ToUpperInvariant();
        if (roleUpper == Role.Instructor)
            overview.TotalInstructors++;
        else if (roleUpper == Role.Student)
            overview.TotalStudents++;

        var now = DateTime.UtcNow;
        var yearMonth = $"{now.Year:D4}-{now.Month:D2}";
        var monthly = await unitOfWork.AggSystemOverviewMonthlyRepository
            .GetOrCreateForMonthAsync(yearMonth, now.Year, now.Month);
        monthly.NewUsers++;
        if (roleUpper == Role.Instructor)
            monthly.NewInstructors++;
        else if (roleUpper == Role.Student)
            monthly.NewStudents++;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("User registered in analytics: UserId {UserId}, Role {Role}",
            message.UserId, message.RoleCode);
    }
}
