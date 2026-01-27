using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Mappings.AiIntegrationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Infrastructure.Configuration;
using Beyond8.Integration.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Beyond8.Integration.Infrastructure.ExternalServices
{
    public class VectorEmbeddingService(
        IHttpClientFactory httpClientFactory,
        QdrantClient qdrantClient,
        IPdfChunkService pdfChunkService,
        IOptions<HuggingFaceSettings> huggingFaceSettings,
        IOptions<QdrantSettings> qdrantSettings,
        ILogger<VectorEmbeddingService> logger) : IVectorEmbeddingService
    {
        private readonly HuggingFaceSettings _hfSettings = huggingFaceSettings.Value;
        private readonly QdrantSettings _qdrantSettings = qdrantSettings.Value;

        public async Task<ApiResponse<List<DocumentEmbeddingResponse>>> EmbedAndSavePdfAsync(
            Stream pdfStream,
            Guid courseId,
            Guid documentId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Chunk PDF
                var chunkResult = pdfChunkService.ChunkPdf(pdfStream, courseId, documentId);
                if (chunkResult.Count == 0)
                {
                    return ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("Không có chunks để embed.");
                }

                const int batchSize = 20; // Số chunks gom lại mỗi batch
                var embeddings = new List<DocumentEmbedding>();

                for (int i = 0; i < chunkResult.Count; i += batchSize)
                {
                    var batch = chunkResult.Skip(i).Take(batchSize).ToList();
                    var texts = batch.Select(c => c.Text).ToArray();

                    var batchResult = await EmbedTextsBatchAsync(texts);
                    if (batchResult == null || batchResult.Count == 0)
                    {
                        logger.LogWarning("Failed to embed batch starting at index {StartIndex} for document {DocumentId}", i, documentId);
                        continue;
                    }

                    var vectors = batchResult;
                    for (int j = 0; j < batch.Count && j < vectors.Count(); j++)
                    {
                        embeddings.Add(batch[j].ToDocumentEmbedding(vectors[j], courseId));
                    }
                }

                if (embeddings.Count == 0)
                {
                    logger.LogWarning("Không thể embed bất kỳ chunk nào.");
                    return ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("Không thể embed bất kỳ chunk nào.");
                }

                // Lưu vào Qdrant
                var upsertResult = await UpsertCourseDocumentsAsync(courseId, embeddings);
                if (!upsertResult)
                {
                    return ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("Không thể lưu chunks vào Qdrant.");
                }

                stopwatch.Stop();
                logger.LogInformation(
                    "Embedded and saved {Count} chunks from PDF to Qdrant for course {CourseId} in {ElapsedMs}ms",
                    embeddings.Count,
                    courseId,
                    stopwatch.ElapsedMilliseconds);

                var responses = embeddings.Select(e => e.ToDocumentEmbeddingResponse(Guid.NewGuid())).ToList();

                return ApiResponse<List<DocumentEmbeddingResponse>>.SuccessResponse(
                    responses,
                    $"Đã embed và lưu {embeddings.Count} chunks từ PDF vào Qdrant.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error embedding PDF for course {CourseId}", courseId);
                return ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("Đã xảy ra lỗi khi embed PDF.");
            }
        }

        public async Task<List<VectorSearchResult>> SearchAsync(VectorSearchRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    logger.LogWarning("Query cannot be empty.");
                    return [];
                }

                // Embed query text
                var embeddingResult = await EmbedTextAsync(request.Query);
                if (embeddingResult == null)
                {
                    logger.LogWarning("Failed to embed query text.");
                    return [];
                }

                // Search trong Qdrant
                var searchResult = await SearchInCourseAsync(
                    request.CourseId,
                    embeddingResult!.Vector,
                    request.TopK,
                    request.ScoreThreshold);

                stopwatch.Stop();
                logger.LogInformation(
                    "Searched course {CourseId} with query in {ElapsedMs}ms, found {Count} results",
                    request.CourseId,
                    stopwatch.ElapsedMilliseconds,
                    searchResult?.Count ?? 0);

                return searchResult ?? [];
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error searching with query for course {CourseId}", request.CourseId);
                return [];
            }
        }


        private async Task<bool> UpsertCourseDocumentsAsync(
            Guid courseId,
            List<DocumentEmbedding> embeddings)
        {
            var stopwatch = Stopwatch.StartNew();
            var collectionName = GetCollectionName(courseId);

            try
            {
                // Ensure collection exists
                await EnsureCollectionExistsAsync(courseId);

                var points = embeddings.Select((embedding, index) =>
                {
                    var pointId = embedding.Metadata.ContainsKey("Id") &&
                                 Guid.TryParse(embedding.Metadata["Id"].ToString(), out var id)
                        ? id.ToString()
                        : Guid.NewGuid().ToString();

                    var payload = new Dictionary<string, Value>
                    {
                        ["courseId"] = courseId.ToString(),
                        ["documentId"] = embedding.DocumentId.ToString(),
                        ["pageNumber"] = embedding.PageNumber,
                        ["chunkIndex"] = embedding.ChunkIndex,
                        ["text"] = embedding.Text,
                        ["createdAt"] = DateTime.UtcNow.ToString("O")
                    };

                    // Add lessonId if present
                    if (embedding.LessonId.HasValue)
                    {
                        payload["lessonId"] = embedding.LessonId.Value.ToString();
                    }

                    // Add metadata fields
                    foreach (var meta in embedding.Metadata)
                    {
                        if (meta.Value != null)
                        {
                            payload[meta.Key] = ConvertToValue(meta.Value);
                        }
                    }

                    var point = new PointStruct
                    {
                        Id = new PointId { Uuid = pointId },
                        Vectors = embedding.Vector
                    };

                    // Set payload using SetPayload method
                    foreach (var kvp in payload)
                    {
                        point.Payload[kvp.Key] = kvp.Value;
                    }

                    return point;
                }).ToList();

                await qdrantClient.UpsertAsync(collectionName, points);

                stopwatch.Stop();
                logger.LogInformation(
                    "Upserted {Count} documents to course {CourseId} collection in {ElapsedMs}ms",
                    embeddings.Count,
                    courseId,
                    stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error upserting documents to course {CourseId}", courseId);
                return false;
            }
        }


        private async Task<List<VectorSearchResult>> SearchInCourseAsync(
            Guid courseId,
            float[] queryVector,
            int topK = 5,
            double? scoreThreshold = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var collectionName = GetCollectionName(courseId);

            try
            {
                // Check if collection exists
                var exists = await CourseCollectionExistsAsync(courseId);
                if (!exists)
                {
                    logger.LogWarning("Collection {CollectionName} does not exist", collectionName);
                    return [];
                }

                var conditions = new List<Condition>
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "courseId",
                            Match = new Match
                            {
                                Text = courseId.ToString()
                            }
                        }
                    }
                };


                var searchFilter = new Filter
                {
                    Must = { conditions }
                };

                var searchResult = await qdrantClient.SearchAsync(
                    collectionName,
                    queryVector,
                    limit: (ulong)topK,
                    scoreThreshold: scoreThreshold.HasValue ? (float?)scoreThreshold.Value : null,
                    filter: searchFilter);

                var results = searchResult.Select(point => point.ToVectorSearchResult()).ToList();

                stopwatch.Stop();
                logger.LogInformation(
                    "Searched course {CourseId} collection, found {Count} results in {ElapsedMs}ms",
                    courseId,
                    results.Count,
                    stopwatch.ElapsedMilliseconds);

                return results;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Error searching in course {CourseId}", courseId);
                return [];
            }
        }

        public async Task<ApiResponse<bool>> CheckHealthAsync()
        {
            try
            {
                // Check Qdrant connection
                try
                {
                    var collections = await qdrantClient.ListCollectionsAsync();
                    logger.LogDebug("Qdrant health check: Successfully listed {Count} collections", collections?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Qdrant health check failed");
                    return ApiResponse<bool>.FailureResponse("Không thể kết nối đến Qdrant.");
                }

                // Check Hugging Face API
                try
                {
                    var testText = "health check";
                    var embeddingResult = await EmbedTextAsync(testText);
                    if (embeddingResult == null || embeddingResult.Vector.Length == 0)
                    {
                        logger.LogWarning("Hugging Face health check: Failed to embed test text");
                        return ApiResponse<bool>.FailureResponse("Không thể embed test text.");
                    }
                    logger.LogDebug("Hugging Face health check: Successfully embedded test text, vector dimension: {Dimension}", embeddingResult.Dimension);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Hugging Face health check failed");
                    return ApiResponse<bool>.FailureResponse("Không thể kết nối đến Hugging Face.");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Dịch vụ embedding khỏe mạnh.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking embedding service health");
                return ApiResponse<bool>.FailureResponse("Không thể kiểm tra tình trạng dịch vụ embedding.");
            }
        }

        private async Task<bool> CourseCollectionExistsAsync(Guid courseId)
        {
            try
            {
                var collectionName = GetCollectionName(courseId);
                var collections = await qdrantClient.ListCollectionsAsync();
                var exists = collections.Contains(collectionName);

                return exists;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking if collection exists for course {CourseId}", courseId);
                return false;
            }
        }

        private async Task EnsureCollectionExistsAsync(Guid courseId)
        {
            var collectionName = GetCollectionName(courseId);
            var exists = await CourseCollectionExistsAsync(courseId);

            if (!exists)
            {
                await qdrantClient.CreateCollectionAsync(
                    collectionName,
                    new VectorParams
                    {
                        Size = (ulong)_qdrantSettings.VectorDimension,
                        Distance = Distance.Cosine
                    });

                logger.LogInformation("Created collection {CollectionName} for course {CourseId}", collectionName, courseId);
            }
        }

        private async Task<List<float[]>> EmbedTextsBatchAsync(string[] texts)
        {
            if (texts == null || texts.Length == 0)
            {
                return [];
            }

            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_hfSettings.TimeoutSeconds);

            if (!string.IsNullOrEmpty(_hfSettings.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _hfSettings.ApiKey);
            }

            var url = $"{_hfSettings.ApiEndpoint.TrimEnd('/')}/{_hfSettings.DefaultModel}";

            // Gửi tất cả texts trong một request
            var requestBody = JsonSerializer.Serialize(new
            {
                inputs = texts,
                options = new { wait_for_model = true }
            });

            HttpResponseMessage? response = null;
            var lastResponseContent = string.Empty;

            for (var attempt = 0; attempt <= _hfSettings.MaxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    var delayMs = (int)(Math.Pow(2, attempt) * 1000);
                    logger.LogWarning("Hugging Face API retry {Attempt}/{Max} after {DelayMs}ms", attempt, _hfSettings.MaxRetries, delayMs);
                    await Task.Delay(delayMs);
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                response = await httpClient.SendAsync(request);
                lastResponseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    break;

                var isRetryable = response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                                 response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                                 response.StatusCode == System.Net.HttpStatusCode.RequestTimeout;

                if (attempt >= _hfSettings.MaxRetries || !isRetryable)
                    break;
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                logger.LogError("Hugging Face API error: {StatusCode} - {Content}", response?.StatusCode, lastResponseContent);
                return [];
            }

            try
            {
                // Parse response - Hugging Face returns array of arrays khi gửi array inputs
                var jsonDoc = JsonDocument.Parse(lastResponseContent);
                var root = jsonDoc.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                {
                    logger.LogError("Unexpected Hugging Face response format: {Content}", lastResponseContent);
                    return [];
                }

                var vectors = new List<float[]>();
                foreach (var element in root.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        var vector = element.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
                        vectors.Add(vector);
                    }
                    else
                    {
                        logger.LogWarning("Unexpected element type in response array");
                    }
                }

                if (vectors.Count != texts.Length)
                {
                    logger.LogWarning("Response vector count ({VectorCount}) does not match input text count ({TextCount})", vectors.Count, texts.Length);
                }

                return vectors;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing Hugging Face response: {Content}", lastResponseContent);
                return [];
            }
        }

        private async Task<EmbeddingResponse?> EmbedTextAsync(string text)
        {
            var batchResult = await EmbedTextsBatchAsync([text]);
            if (batchResult == null || batchResult.Count == 0)
            {
                return null;
            }

            return new EmbeddingResponse
            {
                Vector = batchResult[0],
                Dimension = batchResult[0].Length,
                Model = _hfSettings.DefaultModel
            };
        }

        private static string GetCollectionName(Guid courseId)
        {
            return $"course_{courseId:N}";
        }

        private static Value ConvertToValue(object value)
        {
            return value switch
            {
                string str => str,
                int i => i,
                long l => (int)l,
                double d => d,
                float f => f,
                bool b => b,
                Guid g => g.ToString(),
                _ => value?.ToString() ?? string.Empty
            };
        }
    }
}
