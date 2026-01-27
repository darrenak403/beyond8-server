using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings;

public static class EmbeddingMappings
{
    /// <summary>
    /// Convert DocumentChunk to DocumentEmbedding with vector
    /// </summary>
    public static DocumentEmbedding ToDocumentEmbedding(this DocumentChunk chunk, float[] vector, Guid courseId, Guid? lessonId = null)
    {
        return new DocumentEmbedding
        {
            Vector = vector,
            CourseId = courseId,
            DocumentId = chunk.DocumentId,
            LessonId = lessonId,
            PageNumber = chunk.PageNumber,
            ChunkIndex = chunk.ChunkIndex,
            Text = chunk.Text,
            Metadata = new Dictionary<string, object>
            {
                ["sectionTitle"] = chunk.SectionTitle ?? string.Empty
            }
        };
    }

    /// <summary>
    /// Convert DocumentEmbedding to DocumentEmbeddingResponse
    /// </summary>
    public static DocumentEmbeddingResponse ToDocumentEmbeddingResponse(this DocumentEmbedding embedding, Guid id)
    {
        return new DocumentEmbeddingResponse
        {
            Id = id,
            CourseId = embedding.CourseId,
            DocumentId = embedding.DocumentId,
            LessonId = embedding.LessonId,
            PageNumber = embedding.PageNumber,
            ChunkIndex = embedding.ChunkIndex,
            Text = embedding.Text,
            Metadata = embedding.Metadata
        };
    }

}
