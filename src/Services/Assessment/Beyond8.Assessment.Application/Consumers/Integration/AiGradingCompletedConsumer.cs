using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Consumers.Integration;

public class AiGradingCompletedConsumer(
    ILogger<AiGradingCompletedConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<AiGradingCompletedEvent>
{
    public async Task Consume(ConsumeContext<AiGradingCompletedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Consuming AiGradingCompletedEvent: SubmissionId={SubmissionId}, IsSuccess={IsSuccess}, Score={Score}",
            message.SubmissionId, message.IsSuccess, message.AiScore);

        try
        {
            var submission = await unitOfWork.AssignmentSubmissionRepository
                .FindOneAsync(s => s.Id == message.SubmissionId);

            if (submission == null)
            {
                logger.LogWarning(
                    "Submission not found: SubmissionId={SubmissionId}",
                    message.SubmissionId);
                return;
            }

            if (message.IsSuccess)
            {
                submission.AiScore = message.AiScore;
                submission.AiFeedback = message.AiFeedback;
                submission.Status = SubmissionStatus.AiGraded;

                logger.LogInformation(
                    "AI grading successful for SubmissionId={SubmissionId}. Score: {Score}",
                    message.SubmissionId, message.AiScore);
            }
            else
            {
                submission.Status = SubmissionStatus.ManualReview;
                submission.AiFeedback = message.ErrorMessage ?? "AI grading failed";

                logger.LogWarning(
                    "AI grading failed for SubmissionId={SubmissionId}. Error: {Error}. Status changed to ManualReview.",
                    message.SubmissionId, message.ErrorMessage);
            }

            await unitOfWork.AssignmentSubmissionRepository.UpdateAsync(submission.Id, submission);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Submission updated: SubmissionId={SubmissionId}, Status={Status}",
                message.SubmissionId, submission.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming AiGradingCompletedEvent: SubmissionId={SubmissionId}",
                message.SubmissionId);
            throw;
        }
    }
}
