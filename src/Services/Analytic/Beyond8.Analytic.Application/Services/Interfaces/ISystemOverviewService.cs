using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Services.Interfaces;

public interface ISystemOverviewService
{
    Task<ApiResponse<SystemDashboardResponse>> GetSystemDashboardAsync();
    Task<ApiResponse<RevenueTrendResponse>> GetRevenueTrendAsync(RevenueTrendRequest request);
    Task<ApiResponse<BackfillRevenueResponse>> BackfillRevenueAsync(DateTime from, DateTime to);
}
