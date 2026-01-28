using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings
{
    public static class QuizGenMappings
    {
        private const int DefaultTopK = 15;

        public static VectorSearchRequest ToVectorSearchRequest(this GenQuizRequest request, int defaultTopK = DefaultTopK)
        {
            return new VectorSearchRequest
            {
                CourseId = request.CourseId,
                Query = string.IsNullOrWhiteSpace(request.Query) ? "nội dung khóa học" : request.Query,
                TopK = request.TopK > 0 ? request.TopK : defaultTopK
            };
        }
    }
}
