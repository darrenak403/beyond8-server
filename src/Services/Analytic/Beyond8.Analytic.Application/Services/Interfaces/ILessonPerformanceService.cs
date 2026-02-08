using Beyond8.Analytic.Application.Dtos.LessonPerformance;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface ILessonPerformanceService
{
    Task<ApiResponse<List<LessonPerformanceResponse>>> GetLessonPerformanceByCourseAsync(Guid courseId);
}
