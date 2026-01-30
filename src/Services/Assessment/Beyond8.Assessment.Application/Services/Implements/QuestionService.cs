using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Mappings.QuestionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements
{
    public class QuestionService(ILogger<QuestionService> logger, IUnitOfWork unitOfWork) : IQuestionService
    {
        public async Task<ApiResponse<List<Guid>>> ImportQuestionsFromAiAsync(QuestionFromAiRequest request, Guid instructorId)
        {
            try
            {
                var newQuestions = request.Easy
                    .Concat(request.Medium)
                    .Concat(request.Hard)
                    .Select(q => q.ToEntity(instructorId, QuestionType.MultipleChoice))
                    .ToList();

                await unitOfWork.QuestionRepository.AddRangeAsync(newQuestions);

                logger.LogInformation("Questions imported successfully for instructor {InstructorId}", instructorId);
                return ApiResponse<List<Guid>>.SuccessResponse([.. newQuestions.Select(q => q.Id)], "Câu hỏi đã được nhập thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing questions from AI");
                return ApiResponse<List<Guid>>.FailureResponse("Đã xảy ra lỗi khi nhập câu hỏi từ AI");
            }
        }
    }
}