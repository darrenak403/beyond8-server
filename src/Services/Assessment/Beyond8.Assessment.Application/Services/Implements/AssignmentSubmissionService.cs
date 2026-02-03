using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Application.Mappings.AssignmentSubmissionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class AssignmentSubmissionService(
    ILogger<AssignmentSubmissionService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IAssignmentSubmissionService
{
    public async Task<ApiResponse<SubmissionResponse>> CreateSubmissionAsync(Guid assignmentId, CreateSubmissionRequest request, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == assignmentId);
            if (assignment == null)
                return ApiResponse<SubmissionResponse>.FailureResponse("Assignment không tồn tại.");

            var existingSubmissions = await unitOfWork.AssignmentSubmissionRepository
                .GetAllAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (existingSubmissions.Count >= assignment.TotalSubmissions && assignment.TotalSubmissions > 0)
                return ApiResponse<SubmissionResponse>.FailureResponse("Bạn đã đạt số lượng nộp bài tối đa cho assignment này.");

            var submission = request.ToEntity(assignmentId, userId, existingSubmissions.Count + 1);
            await unitOfWork.AssignmentSubmissionRepository.AddAsync(submission);

            assignment.TotalSubmissions = existingSubmissions.Count + 1;
            await unitOfWork.AssignmentRepository.UpdateAsync(assignmentId, assignment);
            await unitOfWork.SaveChangesAsync();

            if (assignment.GradingMode == GradingMode.AiAssisted)
            {
                submission.Status = SubmissionStatus.AiGrading;
                await unitOfWork.AssignmentSubmissionRepository.UpdateAsync(submission.Id, submission);
                await unitOfWork.SaveChangesAsync();

                await publishEndpoint.Publish(new AssignmentSubmittedEvent(
                    SubmissionId: submission.Id,
                    AssignmentId: assignment.Id,
                    StudentId: userId,
                    AssignmentTitle: assignment.Title,
                    AssignmentDescription: assignment.Description,
                    TextContent: submission.TextContent,
                    FileUrls: submission.FileUrls,
                    RubricUrl: assignment.RubricUrl,
                    TotalPoints: assignment.TotalPoints,
                    SubmittedAt: submission.SubmittedAt
                ));

                logger.LogInformation(
                    "Assignment submission {SubmissionId} created and sent for AI grading. Assignment: {AssignmentId}, Student: {StudentId}",
                    submission.Id, assignmentId, userId);
            }

            return ApiResponse<SubmissionResponse>.SuccessResponse(
                submission.ToResponse(),
                "Nộp bài thành công. Bài làm đang được chấm điểm.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating submission for assignment {AssignmentId}", assignmentId);
            return ApiResponse<SubmissionResponse>.FailureResponse("Đã xảy ra lỗi khi tạo submission.");
        }
    }
}
