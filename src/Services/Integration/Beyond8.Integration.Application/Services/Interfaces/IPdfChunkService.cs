using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    /// <summary>
    /// Service để parse PDF và chunk thành các DocumentChunk
    /// </summary>
    public interface IPdfChunkService
    {
        /// <summary>
        /// Parse PDF file và chunk thành các DocumentChunk
        /// </summary>
        List<DocumentChunk> ChunkPdf(
            Stream pdfStream,
            Guid courseId,
            Guid documentId,
            int chunkSize = 500,
            int chunkOverlap = 50);
    }
}
