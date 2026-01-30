using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings
{
    public static class QuizGenMappings
    {
        private const int DefaultTopK = 15;

        public static VectorSearchRequest ToVectorSearchRequest(this GenQuizRequest request, int defaultTopK = DefaultTopK)
        {
            var query = request.Query?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                query = request.LessonId.HasValue ? "nội dung bài học" : "nội dung khóa học";

            return new VectorSearchRequest
            {
                CourseId = request.CourseId,
                LessonId = request.LessonId,
                Query = query,
                TopK = request.TopK > 0 ? request.TopK : defaultTopK
            };
        }
    }
}
