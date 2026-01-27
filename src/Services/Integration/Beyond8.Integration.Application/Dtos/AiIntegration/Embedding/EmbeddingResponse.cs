namespace Beyond8.Integration.Application.Dtos.AiIntegration.Embedding
{
    public class EmbeddingResponse
    {
        public float[] Vector { get; set; } = [];
        public int Dimension { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}
