namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class EmbedCourseDocumentsResult
    {
        public int TotalChunks { get; set; }

        /// <summary>
        /// True if the document was already embedded (skipped)
        /// </summary>
        public bool AlreadyEmbedded { get; set; }
    }
}
