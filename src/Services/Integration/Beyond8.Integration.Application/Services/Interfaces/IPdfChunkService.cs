using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IPdfChunkService
    {
        List<DocumentChunk> ChunkPdf(
            Stream pdfStream,
            Guid courseId,
            Guid documentId,
            int chunkSize = 500,
            int chunkOverlap = 15);
    }
}
