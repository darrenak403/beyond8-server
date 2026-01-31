using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IQuizAttemptService
{
    Task<ApiResponse<StartQuizResponse>> CreateQuizAttemptAsync(Guid quizId, Guid studentId);

    Task<ApiResponse<QuizResultResponse>> SubmitQuizAttemptAsync(Guid attemptId, SubmitQuizRequest request, Guid studentId);

    Task<ApiResponse<QuizResultResponse>> GetQuizAttemptResultAsync(Guid attemptId, Guid studentId);

    Task<ApiResponse<UserQuizAttemptsResponse>> GetUserQuizAttemptsAsync(Guid quizId, Guid studentId);

    Task<ApiResponse<List<Guid>>> FlagQuestionAsync(Guid attemptId, FlagQuestionRequest request, Guid studentId);
}
