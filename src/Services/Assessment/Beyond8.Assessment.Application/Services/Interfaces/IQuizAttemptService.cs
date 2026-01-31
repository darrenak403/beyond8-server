using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IQuizAttemptService
{
    /// <summary>
    /// Creates a new quiz attempt for a student (starts the quiz)
    /// </summary>
    /// <param name="quizId">Quiz ID to attempt</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Quiz attempt with shuffled questions (without correct answers)</returns>
    Task<ApiResponse<StartQuizResponse>> CreateQuizAttemptAsync(Guid quizId, Guid studentId);

    /// <summary>
    /// Submits a quiz attempt with student's answers
    /// </summary>
    /// <param name="attemptId">Attempt ID</param>
    /// <param name="request">Student's answers</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Quiz result with score and detailed feedback</returns>
    Task<ApiResponse<QuizResultResponse>> SubmitQuizAttemptAsync(Guid attemptId, SubmitQuizRequest request, Guid studentId);

    /// <summary>
    /// Gets the result of a completed quiz attempt
    /// </summary>
    /// <param name="attemptId">Attempt ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Quiz result with score and detailed feedback</returns>
    Task<ApiResponse<QuizResultResponse>> GetQuizAttemptResultAsync(Guid attemptId, Guid studentId);
}
