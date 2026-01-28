namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class DocumentChunk
    {
        public string Text { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Guid DocumentId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string? SectionTitle { get; set; }
    }
}
