using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Clients.Catalog;

public interface ICatalogClient : IBaseClient
{
    Task<ApiResponse<PlatformCourseStatsResponse>> GetPlatformCourseStatsAsync();
    Task<ApiResponse<InstructorCourseStatsResponse>> GetInstructorCourseStatsAsync(Guid instructorId);
}
