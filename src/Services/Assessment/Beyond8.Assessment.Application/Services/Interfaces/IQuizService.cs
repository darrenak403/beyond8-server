using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IQuizService
{
    Task<ApiResponse<QuizResponse>> CreateQuizAsync(CreateQuizRequest request, Guid instructorId);
}
