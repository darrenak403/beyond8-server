using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<ApiResponse<EmbedCourseDocumentsResult>> EmbedCourseDocumentsAsync(
            Stream pdfStream,
            EmbedCourseDocumentsRequest request,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> CheckHealthAsync(CancellationToken cancellationToken = default);
    }
}
