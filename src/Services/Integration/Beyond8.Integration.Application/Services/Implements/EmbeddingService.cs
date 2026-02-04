using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements
{
    public class EmbeddingService(
        IVectorEmbeddingService vectorEmbeddingService,
        ILogger<EmbeddingService> logger) : IEmbeddingService
    {
        public async Task<ApiResponse<EmbedCourseDocumentsResult>> EmbedCourseDocumentsAsync(
            Stream pdfStream,
            EmbedCourseDocumentsRequest request,
            string s3Key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await vectorEmbeddingService.EmbedAndSavePdfAsync(
                    pdfStream,
                    request.CourseId,
                    request.DocumentId,
                    s3Key,
                    request.LessonId);

                logger.LogInformation(
                    "Embedded course documents for CourseId {CourseId}, DocumentId {DocumentId}, S3Key {S3Key}, TotalChunks {TotalChunks}",
                    request.CourseId,
                    request.DocumentId,
                    s3Key,
                    data.Count);

                var result = new EmbedCourseDocumentsResult { TotalChunks = data.Count };
                return ApiResponse<EmbedCourseDocumentsResult>.SuccessResponse(
                    result,
                    $"Đã embed và lưu {data.Count} chunks từ PDF vào Qdrant.");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex,
                    "Embed failed for CourseId {CourseId}, DocumentId {DocumentId}, S3Key {S3Key}",
                    request.CourseId,
                    request.DocumentId,
                    s3Key);
                return ApiResponse<EmbedCourseDocumentsResult>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error embedding course documents for CourseId {CourseId}, DocumentId {DocumentId}, S3Key {S3Key}",
                    request.CourseId,
                    request.DocumentId,
                    s3Key);
                return ApiResponse<EmbedCourseDocumentsResult>.FailureResponse(
                    "Đã xảy ra lỗi khi embed tài liệu khóa học.");
            }
        }

        public async Task<bool> S3KeyExistsAsync(Guid courseId, string s3Key)
        {
            return await vectorEmbeddingService.S3KeyExistsInCollectionAsync(courseId, s3Key);
        }

        public async Task<ApiResponse<bool>> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var healthy = await vectorEmbeddingService.CheckHealthAsync();
                return ApiResponse<bool>.SuccessResponse(healthy, "Dịch vụ embedding khỏe mạnh.");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponse<bool>.FailureResponse(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking embedding service health");
                return ApiResponse<bool>.FailureResponse("Không thể kiểm tra tình trạng dịch vụ embedding.");
            }
        }
    }
}
