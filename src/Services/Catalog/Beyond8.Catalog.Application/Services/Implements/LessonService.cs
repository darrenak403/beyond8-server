using System;
using System.Net.Http.Json;
using System.Text.Json;
using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class LessonService(ILogger<LessonService> logger, IUnitOfWork unitOfWork) : ILessonService
{
    public async Task<ApiResponse<bool>> CallbackHlsAsync(VideoCallbackDto request)
    {
        try
        {
            logger.LogInformation("Callback HLS for original key: {OriginalKey}", request.OriginalKey);
            logger.LogInformation("Transcoding data: {TranscodingData}", JsonSerializer.Serialize(request.TranscodingData));
            var lesson = await unitOfWork.LessonRepository.FindOneAsync(x => string.IsNullOrEmpty(x.VideoOriginalUrl) || x.VideoOriginalUrl.Contains(request.OriginalKey));
            if (lesson == null)
            {
                logger.LogWarning("Lesson not found with original key: {OriginalKey}", request.OriginalKey);
                return ApiResponse<bool>.FailureResponse("Lesson not found");
            }
            lesson.HlsVariants = JsonSerializer.Serialize(request.TranscodingData.Variants);
            await unitOfWork.LessonRepository.UpdateAsync(lesson.Id, lesson);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Callback HLS success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CallbackHlsAsync");
            return ApiResponse<bool>.FailureResponse("Error in CallbackHlsAsync");
        }
    }
}
