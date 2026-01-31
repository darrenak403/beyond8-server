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
    public async Task<ApiResponse<QuizResponse>> CreateQuizAsync(CreateQuizRequest request, Guid instructorId)
    {
        if (request.QuestionIds == null || request.QuestionIds.Count == 0)
            return ApiResponse<QuizResponse>.FailureResponse("Quiz phải có ít nhất một câu hỏi.");

        var questionIdsSet = request.QuestionIds.ToHashSet();
        var questions = await unitOfWork.QuestionRepository.GetAllAsync(q =>
            questionIdsSet.Contains(q.Id) && q.InstructorId == instructorId);

        if (questions.Count != request.QuestionIds.Count)
            return ApiResponse<QuizResponse>.FailureResponse(
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

        return ApiResponse<QuizResponse>.SuccessResponse(
            quiz.ToResponse(request.QuestionIds.Count),
            "Tạo quiz thành công.");
    }
}
