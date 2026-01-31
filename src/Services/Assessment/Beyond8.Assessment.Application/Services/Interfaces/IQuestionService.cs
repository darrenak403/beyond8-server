using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<ApiResponse<QuestionResponse>> CreateQuestionAsync(QuestionRequest request, Guid userId);
        Task<ApiResponse<List<QuestionResponse>>> CreateQuestionsAsync(List<QuestionRequest> requests, Guid userId);
        Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id, Guid userId);
        Task<ApiResponse<List<Guid>>> ImportQuestionsFromAiAsync(QuestionFromAiRequest request, Guid instructorId);
        Task<ApiResponse<List<QuestionResponse>>> GetQuestionsAsync(GetQuestionsRequest request, Guid instructorId);
        Task<ApiResponse<List<TagCountResponse>>> GetTagCountsAsync(Guid instructorId);
        Task<ApiResponse<bool>> UpdateQuestionAsync(Guid id, QuestionRequest request, Guid userId);
    }
}