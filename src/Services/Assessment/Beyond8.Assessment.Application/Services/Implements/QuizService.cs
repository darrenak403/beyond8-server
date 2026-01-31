using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Application.Mappings.QuizMappings;
using Beyond8.Assessment.Application.Mappings.QuizQuestionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class QuizService(
    ILogger<QuizService> logger,
    IUnitOfWork unitOfWork) : IQuizService
{
    public async Task<ApiResponse<QuizResponse>> GetQuizByIdAsync(Guid id, Guid userId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);
            if (quiz == null)
                return ApiResponse<QuizResponse>.FailureResponse("Quiz không tồn tại.");

            var questions = await unitOfWork.QuestionRepository.GetAllAsync(q => quiz.QuizQuestions.Select(qq => qq.QuestionId).Contains(q.Id));
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
            // TODO (cross-service): Không cấm xóa khi có Lesson (Catalog) đang dùng quiz này — Assessment không gọi Catalog.
            // Ảnh hưởng: Lesson.QuizId có thể trỏ tới quiz đã xóa → dangling reference; GET /quizzes/{id} sẽ 404.
            // Bổ sung sau: publish event QuizDeleted sang Catalog để consumer clear Lesson.QuizId cho các lesson có QuizId = id.
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);
            if (quiz == null)
                return ApiResponse<bool>.FailureResponse("Quiz không tồn tại.");

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
