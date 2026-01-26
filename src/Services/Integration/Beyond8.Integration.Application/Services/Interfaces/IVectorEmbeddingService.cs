using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces;

/// <summary>
/// Service gom cả embedding và vector database operations
/// </summary>
public interface IVectorEmbeddingService
{
    /// <summary>
    /// Embed và lưu course PDF documents vào Qdrant (tự động chunk PDF)
    /// </summary>
    Task<ApiResponse<List<DocumentEmbeddingResponse>>> EmbedAndSavePdfAsync(
        Stream pdfStream,
        Guid courseId,
        Guid documentId,
        Guid? lessonId = null);

    /// <summary>
    /// Search trong course với query text (tự động embed query)
    /// </summary>
    Task<List<VectorSearchResult>> SearchAsync(VectorSearchRequest request);

    /// <summary>
    /// Kiểm tra health của embedding service (Qdrant và Hugging Face)
    /// </summary>
    Task<ApiResponse<bool>> CheckHealthAsync();
}
