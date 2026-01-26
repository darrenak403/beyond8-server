using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Qdrant.Client.Grpc;

namespace Beyond8.Integration.Infrastructure.Mappings;

public static class EmbeddingMappings
{
    /// <summary>
    /// Convert Qdrant ScoredPoint to VectorSearchResult
    /// </summary>
    public static VectorSearchResult ToVectorSearchResult(this ScoredPoint point)
    {
        var result = new VectorSearchResult
        {
            Id = Guid.Parse(point.Id.Uuid),
            Score = point.Score,
            CourseId = Guid.Parse(point.Payload["courseId"].StringValue),
            ChunkIndex = (int)point.Payload["chunkIndex"].IntegerValue,
            Text = point.Payload["text"].StringValue,
            Metadata = point.Payload.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.KindCase switch
                {
                    Value.KindOneofCase.StringValue => (object)kvp.Value.StringValue,
                    Value.KindOneofCase.IntegerValue => kvp.Value.IntegerValue,
                    Value.KindOneofCase.DoubleValue => kvp.Value.DoubleValue,
                    Value.KindOneofCase.BoolValue => kvp.Value.BoolValue,
                    _ => kvp.Value.ToString()
                })
        };

        // Parse document fields
        if (point.Payload.TryGetValue("documentId", out var docId))
        {
            result.DocumentId = Guid.Parse(docId.StringValue);
        }

        if (point.Payload.TryGetValue("pageNumber", out var pageNum))
        {
            result.PageNumber = (int)pageNum.IntegerValue;
        }

        // Parse lessonId if present
        if (point.Payload.TryGetValue("lessonId", out var lessonIdValue))
        {
            result.LessonId = Guid.Parse(lessonIdValue.StringValue);
        }

        return result;
    }
}
