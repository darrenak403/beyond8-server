namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class EmbedCourseDocumentsRequest
    {
        public Guid CourseId { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? LessonId { get; set; }
        public string CloudFrontUrl { get; set; } = string.Empty;
    }
}
