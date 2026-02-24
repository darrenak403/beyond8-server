using Beyond8.Assessment.Application.Dtos.Reassign;
using Beyond8.Assessment.Application.Mappings.ReassignMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class ReassignService(
    ILogger<ReassignService> logger,
    IUnitOfWork unitOfWork) : IReassignService
{
    public async Task<ApiResponse<ReassignRequestResponse>> RequestQuizReassignAsync(Guid quizId, RequestQuizReassignRequest request, Guid studentId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == quizId && q.IsActive);
            if (quiz == null)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Quiz không tồn tại hoặc đã bị xóa.");

            var attempts = await unitOfWork.QuizAttemptRepository.GetAllAsync(a => a.QuizId == quizId && a.StudentId == studentId);
            if (attempts.Count == 0)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Bạn chưa có lượt làm nào cho quiz này.");
            if (quiz.MaxAttempts > 0 && attempts.Count < quiz.MaxAttempts)
                return ApiResponse<ReassignRequestResponse>.FailureResponse($"Bạn vẫn còn lượt làm ({attempts.Count}/{quiz.MaxAttempts}). Chỉ có thể yêu cầu reassign khi đã hết lượt.");

            var existing = await unitOfWork.ReassignRequestRepository.FindOneAsync(r =>
                r.Type == ReassignType.Quiz && r.SourceId == quizId && r.StudentId == studentId && r.Status == ReassignRequestStatus.Pending);
            if (existing != null)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Bạn đã có yêu cầu reassign đang chờ xử lý.");

            var now = DateTime.UtcNow;
            var entity = request.ToEntity(quizId, studentId, now);
            await unitOfWork.ReassignRequestRepository.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Quiz reassign requested: QuizId={QuizId}, StudentId={StudentId}, RequestId={RequestId}", quizId, studentId, entity.Id);

            return ApiResponse<ReassignRequestResponse>.SuccessResponse(
                entity.ToResponse("Yêu cầu reassign đã được gửi. Giảng viên sẽ xử lý."),
                "Gửi yêu cầu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting quiz reassign: QuizId={QuizId}, StudentId={StudentId}", quizId, studentId);
            return ApiResponse<ReassignRequestResponse>.FailureResponse("Đã xảy ra lỗi khi gửi yêu cầu.");
        }
    }

    public async Task<ApiResponse<ReassignRequestResponse>> RequestAssignmentReassignAsync(Guid assignmentId, RequestAssignmentReassignRequest request, Guid studentId)
    {
        try
        {
            var assignment = await unitOfWork.AssignmentRepository.FindOneAsync(a => a.Id == assignmentId);
            if (assignment == null)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Assignment không tồn tại.");

            var submissions = await unitOfWork.AssignmentSubmissionRepository.GetAllAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
            if (submissions.Count == 0)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Bạn chưa có lượt nộp nào cho assignment này.");
            if (assignment.MaxSubmissions > 0 && submissions.Count < assignment.MaxSubmissions)
                return ApiResponse<ReassignRequestResponse>.FailureResponse($"Bạn vẫn còn lượt nộp ({submissions.Count}/{assignment.MaxSubmissions}). Chỉ có thể yêu cầu reassign khi đã hết lượt.");

            var existing = await unitOfWork.ReassignRequestRepository.FindOneAsync(r =>
                r.Type == ReassignType.Assignment && r.SourceId == assignmentId && r.StudentId == studentId && r.Status == ReassignRequestStatus.Pending);
            if (existing != null)
                return ApiResponse<ReassignRequestResponse>.FailureResponse("Bạn đã có yêu cầu reassign đang chờ xử lý.");

            var now = DateTime.UtcNow;
            var entity = request.ToEntity(assignmentId, studentId, now);
            await unitOfWork.ReassignRequestRepository.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Assignment reassign requested: AssignmentId={AssignmentId}, StudentId={StudentId}, RequestId={RequestId}", assignmentId, studentId, entity.Id);

            return ApiResponse<ReassignRequestResponse>.SuccessResponse(
                entity.ToResponse("Yêu cầu reassign đã được gửi. Giảng viên sẽ xử lý."),
                "Gửi yêu cầu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting assignment reassign: AssignmentId={AssignmentId}, StudentId={StudentId}", assignmentId, studentId);
            return ApiResponse<ReassignRequestResponse>.FailureResponse("Đã xảy ra lỗi khi gửi yêu cầu.");
        }
    }

    public async Task RecordQuizResetAsync(Guid quizId, Guid studentId, Guid instructorId, Guid? lessonId, int deletedCount)
    {
        var resetAt = DateTime.UtcNow;
        var history = ReassignMappings.ToQuizResetHistory(quizId, studentId, instructorId, lessonId, deletedCount, resetAt);
        var pendingRequest = await unitOfWork.ReassignRequestRepository.FindOneAsync(r =>
            r.Type == ReassignType.Quiz && r.SourceId == quizId && r.StudentId == studentId && r.Status == ReassignRequestStatus.Pending);
        if (pendingRequest != null)
        {
            pendingRequest.Status = ReassignRequestStatus.Approved;
            pendingRequest.ProcessedBy = instructorId;
            pendingRequest.ProcessedAt = resetAt;
            await unitOfWork.ReassignRequestRepository.UpdateAsync(pendingRequest.Id, pendingRequest);
            history.ReassignRequestId = pendingRequest.Id;
        }
        await unitOfWork.ReassignHistoryRepository.AddAsync(history);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RecordAssignmentResetAsync(Guid assignmentId, Guid studentId, Guid instructorId, Guid? sectionId, int deletedCount)
    {
        var resetAt = DateTime.UtcNow;
        var history = ReassignMappings.ToAssignmentResetHistory(assignmentId, studentId, instructorId, sectionId, deletedCount, resetAt);
        var pendingRequest = await unitOfWork.ReassignRequestRepository.FindOneAsync(r =>
            r.Type == ReassignType.Assignment && r.SourceId == assignmentId && r.StudentId == studentId && r.Status == ReassignRequestStatus.Pending);
        if (pendingRequest != null)
        {
            pendingRequest.Status = ReassignRequestStatus.Approved;
            pendingRequest.ProcessedBy = instructorId;
            pendingRequest.ProcessedAt = resetAt;
            await unitOfWork.ReassignRequestRepository.UpdateAsync(pendingRequest.Id, pendingRequest);
            history.ReassignRequestId = pendingRequest.Id;
        }
        await unitOfWork.ReassignHistoryRepository.AddAsync(history);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<ApiResponse<List<ReassignRequestItemDto>>> GetOverviewForInstructorAsync(Guid instructorId, GetReassignOverviewRequest request)
    {
        try
        {
            var quizzes = await unitOfWork.QuizRepository.GetAllAsync(q => q.InstructorId == instructorId);
            var assignments = await unitOfWork.AssignmentRepository.GetAllAsync(a => a.InstructorId == instructorId);
            var quizIds = quizzes.Select(q => q.Id).ToHashSet();
            var assignmentIds = assignments.Select(a => a.Id).ToHashSet();
            var quizTitles = quizzes.ToDictionary(q => q.Id, q => q.Title);
            var assignmentTitles = assignments.ToDictionary(a => a.Id, a => a.Title);

            var pending = await unitOfWork.ReassignRequestRepository.GetAllAsync(r => r.Status == ReassignRequestStatus.Pending);
            var forInstructor = pending.Where(r =>
                (r.Type == ReassignType.Quiz && quizIds.Contains(r.SourceId)) ||
                (r.Type == ReassignType.Assignment && assignmentIds.Contains(r.SourceId))).ToList();

            var items = forInstructor.ToOverviewItems(quizTitles, assignmentTitles);

            // Search: filter by SourceTitle or Note (case insensitive)
            var search = request.Search?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLowerInvariant();
                items = items.Where(x =>
                    (x.SourceTitle != null && x.SourceTitle.ToLowerInvariant().Contains(searchLower)) ||
                    (x.Note != null && x.Note.ToLowerInvariant().Contains(searchLower))).ToList();
            }

            var totalCount = items.Count;

            // Order: RequestedAt desc by default (newest first)
            var ordered = request.IsDescending == false
                ? items.OrderBy(x => x.RequestedAt).ToList()
                : items.OrderByDescending(x => x.RequestedAt).ToList();

            var paged = ordered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return ApiResponse<List<ReassignRequestItemDto>>.SuccessPagedResponse(
                paged,
                totalCount,
                request.PageNumber,
                request.PageSize,
                "Lấy tổng quan yêu cầu reassign thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting reassign overview for instructor: InstructorId={InstructorId}", instructorId);
            return ApiResponse<List<ReassignRequestItemDto>>.FailureResponse("Đã xảy ra lỗi khi lấy tổng quan.");
        }
    }
}
