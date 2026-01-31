using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Mappings.QuestionMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements
{
    public class QuestionService(ILogger<QuestionService> logger, IUnitOfWork unitOfWork) : IQuestionService
    {

        public async Task<ApiResponse<bool>> DeleteQuestionAsync(Guid id, Guid userId)
        {
            try
            {
                var question = await unitOfWork.QuestionRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);
                if (question == null)
                {
                    logger.LogWarning("Question not found: {QuestionId}", id);
                    return ApiResponse<bool>.FailureResponse("Câu hỏi không tồn tại.");
                }

                question.IsActive = false;
                await unitOfWork.QuestionRepository.UpdateAsync(id, question);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Question deleted successfully: {QuestionId}", id);
                return ApiResponse<bool>.SuccessResponse(true, "Câu hỏi đã được xóa thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting question");
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa câu hỏi");
            }
        }

        public async Task<ApiResponse<bool>> UpdateQuestionAsync(Guid id, QuestionRequest request, Guid userId)
        {
            try
            {
                var question = await unitOfWork.QuestionRepository.FindOneAsync(q => q.Id == id && q.InstructorId == userId);

                if (question == null)
                {
                    logger.LogWarning("Question not found: {QuestionId}", id);
                    return ApiResponse<bool>.FailureResponse("Câu hỏi không tồn tại.");
                }

                question.UpdateFromRequest(request);
                await unitOfWork.QuestionRepository.UpdateAsync(id, question);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Question updated successfully: {QuestionId}", id);
                return ApiResponse<bool>.SuccessResponse(true, "Câu hỏi đã được cập nhật thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating question");
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật câu hỏi");
            }
        }

        public async Task<ApiResponse<QuestionResponse>> CreateQuestionAsync(QuestionRequest request, Guid userId)
        {
            try
            {
                var newQuestion = request.ToEntity(userId, request.Type);
                await unitOfWork.QuestionRepository.AddAsync(newQuestion);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<QuestionResponse>.SuccessResponse(newQuestion.ToResponse(), "Câu hỏi đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating question");
                return ApiResponse<QuestionResponse>.FailureResponse("Đã xảy ra lỗi khi tạo câu hỏi");
            }
        }

        public async Task<ApiResponse<List<QuestionResponse>>> CreateQuestionsAsync(List<QuestionRequest> requests, Guid userId)
        {
            try
            {
                var newQuestions = requests.Select(q => q.ToEntity(userId, q.Type)).ToList();
                await unitOfWork.QuestionRepository.AddRangeAsync(newQuestions);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<List<QuestionResponse>>.SuccessResponse([.. newQuestions.Select(q => q.ToResponse())], "Câu hỏi đã được tạo thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating questions");
                return ApiResponse<List<QuestionResponse>>.FailureResponse("Đã xảy ra lỗi khi tạo câu hỏi");
            }
        }

        public async Task<ApiResponse<List<Guid>>> ImportQuestionsFromAiAsync(QuestionFromAiRequest request, Guid instructorId)
        {
            try
            {
                var newQuestions = request.Easy
                    .Concat(request.Medium)
                    .Concat(request.Hard)
                    .Select(q => q.ToEntity(instructorId, q.Type))
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

        public async Task<ApiResponse<List<QuestionResponse>>> GetQuestionsAsync(GetQuestionsRequest request, Guid instructorId)
        {
            try
            {
                var (items, totalCount) = await unitOfWork.QuestionRepository.GetPagedByInstructorAsync(
                    instructorId,
                    request.PageNumber,
                    request.PageSize,
                    string.IsNullOrWhiteSpace(request.Tag) ? null : request.Tag!.Trim(),
                    request.IsDescending ?? true);

                var responses = items.Select(q => q.ToResponse()).ToList();
                return ApiResponse<List<QuestionResponse>>.SuccessPagedResponse(
                    responses,
                    totalCount,
                    request.PageNumber,
                    request.PageSize,
                    "Lấy danh sách câu hỏi thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting questions for instructor {InstructorId}", instructorId);
                return ApiResponse<List<QuestionResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách câu hỏi");
            }
        }

        public async Task<ApiResponse<List<TagCountResponse>>> GetTagCountsAsync(Guid instructorId)
        {
            try
            {
                var tagCounts = await unitOfWork.QuestionRepository.GetTagCountsByInstructorAsync(instructorId);
                var responses = tagCounts.Select(t => new TagCountResponse { Tag = t.Tag, Count = t.Count }).ToList();
                return ApiResponse<List<TagCountResponse>>.SuccessResponse(responses, "Lấy số lượng câu hỏi theo thẻ thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting tag counts for instructor {InstructorId}", instructorId);
                return ApiResponse<List<TagCountResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy số lượng câu hỏi theo thẻ");
            }
        }
    }
}