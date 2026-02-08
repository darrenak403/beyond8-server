using Beyond8.Analytic.Application.Dtos.LessonPerformance;
using Beyond8.Analytic.Application.Mappings;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class LessonPerformanceService(
    ILogger<LessonPerformanceService> logger,
    IUnitOfWork unitOfWork) : ILessonPerformanceService
{
    public async Task<ApiResponse<List<LessonPerformanceResponse>>> GetLessonPerformanceByCourseAsync(Guid courseId)
    {
        var lessons = await unitOfWork.AggLessonPerformanceRepository.GetByCourseIdAsync(courseId);
        var items = lessons.Select(e => e.ToResponse()).ToList();

        return ApiResponse<List<LessonPerformanceResponse>>.SuccessResponse(items, "Lấy thống kê bài học thành công");
    }
}
