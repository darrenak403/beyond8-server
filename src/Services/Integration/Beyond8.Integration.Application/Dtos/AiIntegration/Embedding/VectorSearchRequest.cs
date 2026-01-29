namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class VectorSearchRequest
    {
        public Guid CourseId { get; set; }
        /// <summary>Khi có giá trị, ưu tiên lấy chunks thuộc lesson này.</summary>
        public Guid? LessonId { get; set; }
        public string Query { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
        public double? ScoreThreshold { get; set; }
    }
}
