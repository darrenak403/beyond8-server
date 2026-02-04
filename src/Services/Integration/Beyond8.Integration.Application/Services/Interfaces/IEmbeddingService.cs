using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<ApiResponse<EmbedCourseDocumentsResult>> EmbedCourseDocumentsAsync(
            Stream pdfStream,
            EmbedCourseDocumentsRequest request,
            string s3Key,
            CancellationToken cancellationToken = default);

        Task<bool> S3KeyExistsAsync(Guid courseId, string s3Key);

        Task<ApiResponse<bool>> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
