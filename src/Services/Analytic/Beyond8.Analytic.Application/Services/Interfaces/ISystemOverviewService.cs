using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface ISystemOverviewService
{
    Task<ApiResponse<SystemOverviewResponse>> GetSystemOverviewAsync();
}
