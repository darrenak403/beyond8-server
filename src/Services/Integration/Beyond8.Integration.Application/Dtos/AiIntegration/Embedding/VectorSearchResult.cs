namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;

public class VectorSearchResult
{
    public Guid Id { get; set; }
    public float Score { get; set; }
    public Guid CourseId { get; set; }
    
    // For Course Documents
    public Guid? DocumentId { get; set; }
    public int? PageNumber { get; set; }
    
    // For Lesson Content
    public Guid? LessonId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? LessonDocumentId { get; set; }
    
    // Common
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public string ContentType { get; set; } = "document"; // "document" or "lesson"
    public Dictionary<string, object> Metadata { get; set; } = new();
}
