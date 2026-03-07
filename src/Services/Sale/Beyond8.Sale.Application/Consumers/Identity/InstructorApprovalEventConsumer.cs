using Beyond8.Common.Events.Identity;
using Beyond8.Sale.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Consumers.Identity;

public class InstructorApprovalEventConsumer(
    IInstructorWalletService walletService,
    ILogger<InstructorApprovalEventConsumer> logger) : IConsumer<InstructorApprovalEvent>
{
    public async Task Consume(ConsumeContext<InstructorApprovalEvent> context)
    {
        var message = context.Message;

        try
        {
            logger.LogInformation("Creating wallet for newly approved instructor {UserId}", message.UserId);

            var result = await walletService.CreateWalletAsync(message.UserId);

            if (result.IsSuccess)
            {
                logger.LogInformation("Successfully created wallet for instructor {UserId}", message.UserId);
            }
            else
            {
                logger.LogError("Failed to create wallet for instructor {UserId}: {Error}",
                    message.UserId, result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating wallet for instructor {UserId}", message.UserId);
        }
    }
}