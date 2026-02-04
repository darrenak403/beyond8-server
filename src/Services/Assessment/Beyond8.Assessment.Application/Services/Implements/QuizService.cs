using Beyond8.Assessment.Application.Clients.Catalog;
using Beyond8.Assessment.Application.Clients.Learning;
using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Application.Mappings.QuizMappings;
using Beyond8.Assessment.Application.Mappings.QuizQuestionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class QuizService(
    ILogger<QuizService> logger,
    IUnitOfWork unitOfWork,
    ICatalogService catalogService,
    ILearningClient learningClient) : IQuizService
{
    public async Task<ApiResponse<QuizSimpleResponse>> GetQuizByIdForStudentAsync(Guid id)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.AsQueryable()
                .Where(q => q.Id == id && q.IsActive)
                .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                logger.LogError("Quiz not found for id: {Id} for student", id);
                return ApiResponse<QuizSimpleResponse>.FailureResponse("Quiz không tồn tại cho học sinh.");
            }

            var previewResult = await catalogService.IsLessonPreviewByQuizIdAsync(id);
            if (previewResult.IsSuccess && previewResult.Data)
            {
                logger.LogInformation("Quiz {QuizId} is preview lesson, allowing student access without enrollment", id);
                return ApiResponse<QuizSimpleResponse>.SuccessResponse(quiz.ToSimpleResponse(quiz.QuizQuestions.Count), "Lấy quiz thành công.");
            }

            if (!quiz.CourseId.HasValue)
            {
                logger.LogWarning("Quiz {QuizId} has no CourseId, denying student access", id);
                return ApiResponse<QuizSimpleResponse>.FailureResponse("Quiz không gắn khóa học. Không thể truy cập.");
            }

            var enrollmentResult = await learningClient.IsUserEnrolledInCourseAsync(quiz.CourseId.Value);
            if (!enrollmentResult.IsSuccess)
            {
                logger.LogWarning("Learning client failed for quiz {QuizId}, course {CourseId}: {Message}", id, quiz.CourseId, enrollmentResult.Message);
                return ApiResponse<QuizSimpleResponse>.FailureResponse(enrollmentResult.Message ?? "Không thể kiểm tra đăng ký khóa học.");
            }

            if (!enrollmentResult.Data)
            {
                logger.LogWarning("Student not enrolled in course {CourseId} for quiz {QuizId}", quiz.CourseId, id);
                return ApiResponse<QuizSimpleResponse>.FailureResponse("Bạn chưa đăng ký khóa học. Vui lòng đăng ký khóa học trước khi làm quiz.");
            }

            return ApiResponse<QuizSimpleResponse>.SuccessResponse(quiz.ToSimpleResponse(quiz.QuizQuestions.Count), "Lấy quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting quiz by id for student: {Id}", id);
            return ApiResponse<QuizSimpleResponse>.FailureResponse("Đã xảy ra lỗi khi lấy quiz cho học sinh.");
        }
    }

    public async Task<ApiResponse<List<QuizSimpleResponse>>> GetAllQuizzesAsync(Guid userId, PaginationRequest paginationRequest)
    {
        try
        {
            var quizzes = await unitOfWork.QuizRepository.GetPagedAsync(
                pageNumber: paginationRequest.PageNumber,
                pageSize: paginationRequest.PageSize,
                filter: q => q.InstructorId == userId && q.IsActive,
                orderBy: query => query.OrderByDescending(q => q.CreatedAt));

            return ApiResponse<List<QuizSimpleResponse>>.SuccessPagedResponse(
                [.. quizzes.Items.Select(q => q.ToSimpleResponse(q.QuizQuestions.Count))],
                quizzes.TotalCount,
                paginationRequest.PageNumber,
                paginationRequest.PageSize,
                "Lấy tất cả quizzes thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all quizzes for instructor: {InstructorId}", userId);
            return ApiResponse<List<QuizSimpleResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy tất cả quizzes.");
        }
    }

    public async Task<ApiResponse<QuizResponse>> GetQuizByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.AsQueryable()
                .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                .FirstOrDefaultAsync(q => q.Id == id && q.InstructorId == userId && q.IsActive);
            if (quiz == null)
                return ApiResponse<QuizResponse>.FailureResponse("Quiz không tồn tại.");

            var questions = quiz.QuizQuestions.Select(qq => qq.Question).ToList();
            return ApiResponse<QuizResponse>.SuccessResponse(quiz.ToResponse([.. questions]));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting quiz by id: {Id}", id);
            return ApiResponse<QuizResponse>.FailureResponse("Đã xảy ra lỗi khi lấy quiz theo ID.");
        }
    }

    public async Task<ApiResponse<QuizSimpleResponse>> CreateQuizAsync(CreateQuizRequest request, Guid instructorId)
    {
        try
        {
            var questionIdsSet = request.QuestionIds.ToHashSet();
            var questions = await unitOfWork.QuestionRepository.GetAllAsync(q =>
                questionIdsSet.Contains(q.Id) && q.InstructorId == instructorId);

            if (questions.Count != request.QuestionIds.Count)
                return ApiResponse<QuizSimpleResponse>.FailureResponse(
                    "Một hoặc nhiều câu hỏi không tồn tại hoặc không thuộc quyền sở hữu của bạn.");

            var quiz = request.ToEntity(instructorId);
            await unitOfWork.QuizRepository.AddAsync(quiz);

            for (var i = 0; i < request.QuestionIds.Count; i++)
            {
                var qq = QuizQuestionMappings.ToQuizQuestionEntity(quiz.Id, request.QuestionIds[i], i + 1);
                await unitOfWork.QuizQuestionRepository.AddAsync(qq);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Quiz created: {QuizId}, InstructorId: {InstructorId}, QuestionCount: {Count}",
                quiz.Id, instructorId, request.QuestionIds.Count);

            if (request.LessonId != null)
            {
                var catalogResponse = await catalogService.UpdateQuizForLessonAsync(request.LessonId.Value, quiz.Id);
                if (!catalogResponse.IsSuccess)
                {
                    return ApiResponse<QuizSimpleResponse>.FailureResponse(catalogResponse.Message!);
                }
            }

            return ApiResponse<QuizSimpleResponse>.SuccessResponse(
                quiz.ToSimpleResponse(request.QuestionIds.Count),
                "Tạo quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating quiz: {Request}", request);
            return ApiResponse<QuizSimpleResponse>.FailureResponse("Đã xảy ra lỗi khi tạo quiz.");
        }
    }

    public async Task<ApiResponse<QuizResponse>> UpdateQuizAsync(Guid id, UpdateQuizRequest request, Guid userId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);
            if (quiz == null)
                return ApiResponse<QuizResponse>.FailureResponse("Quiz không tồn tại.");

            var questionIdsSet = request.QuestionIds.ToHashSet();
            var questions = await unitOfWork.QuestionRepository.GetAllAsync(q =>
                questionIdsSet.Contains(q.Id) && q.InstructorId == userId);

            if (questions.Count != request.QuestionIds.Count)
                return ApiResponse<QuizResponse>.FailureResponse(
                    "Một hoặc nhiều câu hỏi không tồn tại hoặc không thuộc quyền sở hữu của bạn.");

            quiz.UpdateFromRequest(request);
            await unitOfWork.QuizRepository.UpdateAsync(id, quiz);

            var existingQuizQuestions = await unitOfWork.QuizQuestionRepository.GetAllAsync(qq => qq.QuizId == id);
            foreach (var qq in existingQuizQuestions)
                await unitOfWork.QuizQuestionRepository.DeleteAsync(qq.Id);

            for (var i = 0; i < request.QuestionIds.Count; i++)
            {
                var qq = QuizQuestionMappings.ToQuizQuestionEntity(id, request.QuestionIds[i], i + 1);
                await unitOfWork.QuizQuestionRepository.AddAsync(qq);
            }

            await unitOfWork.SaveChangesAsync();

            var questionDict = questions.ToDictionary(q => q.Id);
            var questionsOrdered = request.QuestionIds
                .Select(questionId => questionDict[questionId])
                .ToList();

            logger.LogInformation("Quiz updated: {QuizId}, InstructorId: {InstructorId}", id, userId);
            return ApiResponse<QuizResponse>.SuccessResponse(
                quiz.ToResponse(questionsOrdered),
                "Cập nhật quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating quiz: {Id}", id);
            return ApiResponse<QuizResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật quiz.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteQuizAsync(Guid id, Guid userId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);
            if (quiz == null)
                return ApiResponse<bool>.FailureResponse("Quiz không tồn tại.");

            if (quiz.LessonId != null)
                return ApiResponse<bool>.FailureResponse(
                    "Không thể xóa quiz đã gắn với lesson. Vui lòng gỡ quiz khỏi lesson trước.");

            quiz.IsActive = false;
            quiz.DeletedAt = DateTime.UtcNow;
            quiz.DeletedBy = userId;
            await unitOfWork.QuizRepository.UpdateAsync(id, quiz);

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Quiz deleted: {QuizId}, InstructorId: {InstructorId}", id, userId);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting quiz: {Id}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa quiz.");
        }
    }
}
