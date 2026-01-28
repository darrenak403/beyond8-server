using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IVectorEmbeddingService
    {
        Task<ApiResponse<List<DocumentEmbeddingResponse>>> EmbedAndSavePdfAsync(
            Stream pdfStream,
            Guid courseId,
            Guid documentId);

        Task<List<VectorSearchResult>> SearchAsync(VectorSearchRequest request);

        Task<ApiResponse<bool>> CheckHealthAsync();
    }
}
