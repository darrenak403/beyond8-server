using Beyond8.Common.Events.Assessment;
using Beyond8.Integration.Application.Dtos.AiIntegration.Grading;
using Beyond8.Integration.Application.Helpers.AiService;
using Beyond8.Integration.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beyond8.Integration.Application.Consumers.Assessment;

public class AssignmentSubmittedConsumer(
    ILogger<AssignmentSubmittedConsumer> logger,
    IAiService aiService,
    IPublishEndpoint publishEndpoint) : IConsumer<AssignmentSubmittedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task Consume(ConsumeContext<AssignmentSubmittedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Consuming AssignmentSubmittedEvent: SubmissionId={SubmissionId}, AssignmentId={AssignmentId}, StudentId={StudentId}",
            message.SubmissionId, message.AssignmentId, message.StudentId);

        try
        {
            var gradingRequest = new AiGradingRequest
            {
                SubmissionId = message.SubmissionId,
                AssignmentId = message.AssignmentId,
                StudentId = message.StudentId,
                AssignmentTitle = message.AssignmentTitle,
                AssignmentDescription = message.AssignmentDescription,
                TextContent = message.TextContent,
                FileUrls = message.FileUrls,
                RubricUrl = message.RubricUrl,
                TotalPoints = message.TotalPoints
            };

            var gradingResult = await aiService.AiGradingAssignmentAsync(gradingRequest);

            if (gradingResult.IsSuccess && gradingResult.Data != null)
            {
                var feedbackJson = AiServiceGradingHelper.ToFeedbackJson(gradingResult.Data, JsonOptions);
                var gradedAt = DateTime.UtcNow;
                var scorePercent = message.TotalPoints > 0
                    ? (gradingResult.Data.Score / message.TotalPoints) * 100
                    : 0m;

                await publishEndpoint.Publish(new AiGradingCompletedEvent(
                    SubmissionId: message.SubmissionId,
                    AssignmentId: message.AssignmentId,
                    SectionId: message.SectionId,
                    StudentId: message.StudentId,
                    AiScore: gradingResult.Data.Score,
                    ScorePercent: scorePercent,
                    PassScorePercent: message.PassScorePercent,
                    AiFeedback: feedbackJson,
                    IsSuccess: true,
                    ErrorMessage: null,
                    GradedAt: gradedAt
                ));

                await publishEndpoint.Publish(new AiAssignmentGradedEvent(
                    SubmissionId: message.SubmissionId,
                    AssignmentId: message.AssignmentId,
                    SectionId: message.SectionId,
                    StudentId: message.StudentId,
                    AssignmentTitle: message.AssignmentTitle,
                    Score: gradingResult.Data.Score,
                    AiFeedback: feedbackJson,
                    GradedAt: gradedAt
                ));

                logger.LogInformation(
                    "AI grading completed successfully for SubmissionId={SubmissionId}. Score: {Score}/{TotalPoints}",
                    message.SubmissionId, gradingResult.Data.Score, gradingResult.Data.TotalPoints);
            }
            else
            {
                await publishEndpoint.Publish(new AiGradingCompletedEvent(
                    SubmissionId: message.SubmissionId,
                    AssignmentId: message.AssignmentId,
                    SectionId: message.SectionId,
                    StudentId: message.StudentId,
                    AiScore: 0,
                    ScorePercent: 0,
                    PassScorePercent: message.PassScorePercent,
                    AiFeedback: "{}",
                    IsSuccess: false,
                    ErrorMessage: gradingResult.Message ?? "Không thể chấm điểm bằng AI",
                    GradedAt: DateTime.UtcNow
                ));

                logger.LogWarning(
                    "AI grading failed for SubmissionId={SubmissionId}. Error: {Error}",
                    message.SubmissionId, gradingResult.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error consuming AssignmentSubmittedEvent: SubmissionId={SubmissionId}",
                message.SubmissionId);

            await publishEndpoint.Publish(new AiGradingCompletedEvent(
                SubmissionId: message.SubmissionId,
                AssignmentId: message.AssignmentId,
                SectionId: message.SectionId,
                StudentId: message.StudentId,
                AiScore: 0,
                ScorePercent: 0,
                PassScorePercent: message.PassScorePercent,
                AiFeedback: "{}",
                IsSuccess: false,
                ErrorMessage: "Đã xảy ra lỗi khi chấm điểm: " + ex.Message,
                GradedAt: DateTime.UtcNow
            ));

            throw;
        }
    }
}
