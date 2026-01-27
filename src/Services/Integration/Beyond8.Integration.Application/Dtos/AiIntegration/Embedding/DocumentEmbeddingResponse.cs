namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class DocumentEmbeddingResponse
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? LessonId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
