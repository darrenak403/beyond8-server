using System;
using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ILessonService
{
    Task<ApiResponse<bool>> CallbackHlsAsync(VideoCallbackDto request);
}
