using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces;

public interface IQuizService
{
    Task<ApiResponse<QuizSimpleResponse>> CreateQuizAsync(CreateQuizRequest request, Guid instructorId);
    Task<ApiResponse<QuizResponse>> GetQuizByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<QuizResponse>> UpdateQuizAsync(Guid id, UpdateQuizRequest request, Guid userId);
    Task<ApiResponse<bool>> DeleteQuizAsync(Guid id, Guid userId);
    Task<ApiResponse<List<QuizSimpleResponse>>> GetAllQuizzesAsync(Guid userId, PaginationRequest paginationRequest);
    Task<ApiResponse<QuizSimpleResponse>> GetQuizByIdForStudentAsync(Guid id);
}
