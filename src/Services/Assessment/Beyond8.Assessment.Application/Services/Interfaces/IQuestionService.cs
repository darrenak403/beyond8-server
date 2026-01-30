using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<ApiResponse<List<Guid>>> ImportQuestionsFromAiAsync(QuestionFromAiRequest request, Guid instructorId);
    }
}