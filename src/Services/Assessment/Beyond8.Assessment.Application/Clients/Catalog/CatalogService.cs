using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Assessment.Application.Clients.Catalog
{
    public class CatalogService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : BaseClient(httpClient, httpContextAccessor), ICatalogService
    {
        public async Task<ApiResponse<bool>> UpdateQuizForLessonAsync(Guid lessonId, Guid? quizId)
        {
            try
            {
                var response = await PatchAsync<bool>($"/api/v1/lessons/{lessonId}/update-quiz", new { QuizId = quizId });
                return ApiResponse<bool>.SuccessResponse(response, "Quiz updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResponse(ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> UpdateAssignmentForSectionAsync(Guid sectionId, Guid? assignmentId)
        {
            try
            {
                var response = await PatchAsync<bool>($"/api/v1/sections/{sectionId}/update-assignment", new { AssignmentId = assignmentId });
                return ApiResponse<bool>.SuccessResponse(response, "Assignment updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResponse(ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> IsLessonPreviewByQuizIdAsync(Guid quizId)
        {
            try
            {
                var isPreview = await GetAsync<bool>($"/api/v1/lessons/preview-by-quiz/{quizId}");
                return ApiResponse<bool>.SuccessResponse(isPreview, isPreview ? "Lesson là preview." : "Lesson không phải preview.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResponse(ex.Message);
            }
        }
    }
}