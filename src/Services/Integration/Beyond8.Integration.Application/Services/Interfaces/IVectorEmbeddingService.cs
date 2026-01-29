using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IVectorEmbeddingService
    {
        Task<List<DocumentEmbeddingResponse>> EmbedAndSavePdfAsync(
            Stream pdfStream,
            Guid courseId,
            Guid documentId,
            Guid? lessonId = null);

        Task<List<VectorSearchResult>> SearchAsync(VectorSearchRequest request);

        Task<bool> CheckHealthAsync();
    }
}
