using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Application.Mappings.AssignmentSubmissionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Assessment;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
            {
                logger.LogError("Assignment not found for id: {AssignmentId}", assignmentId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Assignment không tồn tại.");
            }

            var existingSubmissions = await unitOfWork.AssignmentSubmissionRepository
                .GetAllAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (existingSubmissions.Count >= assignment.TotalSubmissions && assignment.TotalSubmissions > 0)
            {
                logger.LogError("Student {StudentId} has reached the maximum number of submissions for assignment {AssignmentId}", userId, assignmentId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Bạn đã đạt số lượng nộp bài tối đa cho assignment này.");
            }

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
                    SectionId: assignment.SectionId,
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

            logger.LogInformation("Submission created successfully: {SubmissionId}", submission.Id);
            var submissionWithAssignment = await unitOfWork.AssignmentSubmissionRepository.FindOneWithAssignmentAsync(s => s.Id == submission.Id);
            return ApiResponse<SubmissionResponse>.SuccessResponse(
                submissionWithAssignment!.ToResponse(),
                "Nộp bài thành công. Bài làm đang được chấm điểm.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating submission for assignment {AssignmentId}", assignmentId);
            return ApiResponse<SubmissionResponse>.FailureResponse("Đã xảy ra lỗi khi tạo submission.");
        }
    }

    public async Task<ApiResponse<SubmissionResponse>> InstructorGradingSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid userId)
    {
        try
        {
            var submission = await unitOfWork.AssignmentSubmissionRepository.FindOneAsync(s => s.Id == submissionId);
            if (submission == null)
            {
                logger.LogError("Submission not found for id: {SubmissionId}", submissionId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Submission không tồn tại.");
            }

            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == submission.AssignmentId && a.InstructorId == userId);
            if (assignment == null)
            {
                logger.LogError("Assignment not found for id: {AssignmentId}", submission.AssignmentId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Assignment không tồn tại.");
            }

            submission.UpdateFromRequest(request, userId);
            await unitOfWork.AssignmentSubmissionRepository.UpdateAsync(submission.Id, submission);
            await unitOfWork.SaveChangesAsync();

            var score = submission.FinalScore ?? request.FinalScore;
            var gradedAt = submission.GradedAt ?? DateTime.UtcNow;
            if (assignment.SectionId.HasValue)
            {
                await publishEndpoint.Publish(new AssignmentGradedEvent(
                    SubmissionId: submission.Id,
                    AssignmentId: assignment.Id,
                    AssignmentTitle: assignment.Title,
                    SectionId: assignment.SectionId,
                    StudentId: submission.StudentId,
                    Score: score,
                    GradedAt: gradedAt,
                    GradedBy: userId
                ));
            }

            logger.LogInformation("Submission graded successfully: {SubmissionId}", submission.Id);
            var submissionWithAssignment = await unitOfWork.AssignmentSubmissionRepository.FindOneWithAssignmentAsync(s => s.Id == submission.Id);
            return ApiResponse<SubmissionResponse>.SuccessResponse(
                submissionWithAssignment!.ToResponse(),
                "Chấm điểm submission thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error grading submission {SubmissionId}", submissionId);
            return ApiResponse<SubmissionResponse>.FailureResponse("Đã xảy ra lỗi khi chấm điểm submission.");
        }
    }

    public async Task<ApiResponse<SubmissionResponse>> GetSubmissionByIdAsync(Guid submissionId, Guid userId)
    {
        try
        {
            var submission = await unitOfWork.AssignmentSubmissionRepository.FindOneWithAssignmentAsync(s => s.Id == submissionId && s.StudentId == userId);
            if (submission == null)
            {
                logger.LogError("Submission not found for id: {SubmissionId}", submissionId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Submission không tồn tại.");
            }

            return ApiResponse<SubmissionResponse>.SuccessResponse(submission.ToResponse(), "Lấy submission thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting submission by id: {SubmissionId}", submissionId);
            return ApiResponse<SubmissionResponse>.FailureResponse("Đã xảy ra lỗi khi lấy submission.");
        }
    }

    public async Task<ApiResponse<List<SubmissionResponse>>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId, Guid userId)
    {
        try
        {
            var submissions = await unitOfWork.AssignmentSubmissionRepository.GetAllWithAssignmentAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);
            if (submissions.Count == 0)
            {
                logger.LogError("No submissions found for assignment id: {AssignmentId}", assignmentId);
                return ApiResponse<List<SubmissionResponse>>.FailureResponse("Không có submission nào cho assignment này.");
            }

            return ApiResponse<List<SubmissionResponse>>.SuccessResponse(submissions.Select(s => s.ToResponse()).ToList(), "Lấy danh sách submission thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting submissions by assignment id: {AssignmentId}", assignmentId);
            return ApiResponse<List<SubmissionResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách submission.");
        }
    }

    public async Task<ApiResponse<CourseSubmissionSummaryResponse>> GetAllSubmissionsByCourseIdForInstructorAsync(Guid courseId, Guid userId)
    {
        try
        {
            var submissions = await unitOfWork.AssignmentSubmissionRepository.AsQueryable()
                .Include(s => s.Assignment)
                .Where(s => s.Assignment != null && s.Assignment.CourseId == courseId && s.Assignment.InstructorId == userId)
                .ToListAsync();

            var totalAssignments = await unitOfWork.AssignmentRepository.AsQueryable()
                .CountAsync(a => a.CourseId == courseId && a.InstructorId == userId);

            var sectionGroups = submissions
                .Where(s => s.Assignment.SectionId.HasValue)
                .GroupBy(s => s.Assignment.SectionId!.Value)
                .Select(g => new SectionSubmissionSummary
                {
                    SectionId = g.Key,
                    TotalSubmissions = g.Count(),
                    UngradedSubmissions = g.Count(s => s.Status != SubmissionStatus.Graded)
                })
                .ToList();

            var totalUngradedSections = sectionGroups.Count(s => s.UngradedSubmissions > 0);

            var response = new CourseSubmissionSummaryResponse
            {
                Sections = sectionGroups,
                TotalUngradedSections = totalUngradedSections,
                TotalAssignments = totalAssignments
            };

            return ApiResponse<CourseSubmissionSummaryResponse>.SuccessResponse(
                response,
                "Lấy tổng quan submissions theo sections thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all submissions for course id: {CourseId}", courseId);
            return ApiResponse<CourseSubmissionSummaryResponse>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách submission.");
        }
    }

    public async Task<ApiResponse<List<AssignmentSubmissionDetail>>> GetSubmissionsBySectionIdForInstructorAsync(Guid sectionId, Guid userId)
    {
        try
        {
            var submissions = await unitOfWork.AssignmentSubmissionRepository.AsQueryable()
                .Include(s => s.Assignment)
                .Where(s => s.Assignment != null && s.Assignment.SectionId == sectionId && s.Assignment.InstructorId == userId)
                .ToListAsync();

            if (submissions.Count == 0)
            {
                logger.LogError("No submissions found for section id: {SectionId}", sectionId);
                return ApiResponse<List<AssignmentSubmissionDetail>>.FailureResponse("Không có submission nào cho section này.");
            }

            var assignmentGroups = submissions
                .GroupBy(s => s.AssignmentId)
                .Select(g =>
                {
                    var assignment = g.First().Assignment;
                    return new AssignmentSubmissionDetail
                    {
                        AssignmentId = g.Key,
                        AssignmentTitle = assignment.Title,
                        TotalSubmissions = g.Count(),
                        UngradedSubmissions = g.Count(s => s.Status != SubmissionStatus.Graded),
                        Submissions = g.Select(s => s.ToResponse()).ToList()
                    };
                })
                .ToList();

            return ApiResponse<List<AssignmentSubmissionDetail>>.SuccessResponse(
                assignmentGroups,
                "Lấy chi tiết assignments và submissions theo section thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting submissions for section id: {SectionId}", sectionId);
            return ApiResponse<List<AssignmentSubmissionDetail>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách submission.");
        }
    }

    public async Task<ApiResponse<SubmissionResponse>> GetSubmissionByIdForInstructorAsync(Guid submissionId, Guid userId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.InstructorId == userId);
            if (assignment == null)
            {
                logger.LogError("Assignment not found for instructor: {InstructorId}", userId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Assignment không tồn tại.");
            }

            var submission = await unitOfWork.AssignmentSubmissionRepository.FindOneWithAssignmentAsync(s => s.Id == submissionId && s.AssignmentId == assignment.Id);
            if (submission == null)
            {
                logger.LogError("Submission not found for id: {SubmissionId}", submissionId);
                return ApiResponse<SubmissionResponse>.FailureResponse("Submission không tồn tại.");
            }

            return ApiResponse<SubmissionResponse>.SuccessResponse(submission.ToResponse(), "Lấy submission thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting submission by id for instructor: {SubmissionId}", submissionId);
            return ApiResponse<SubmissionResponse>.FailureResponse("Đã xảy ra lỗi khi lấy submission.");
        }
    }

}
