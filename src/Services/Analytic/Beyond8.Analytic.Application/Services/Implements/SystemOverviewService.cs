using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Application.Mappings;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Services.Implements;

public class SystemOverviewService(
    ILogger<SystemOverviewService> logger,
    IUnitOfWork unitOfWork) : ISystemOverviewService
{
    public async Task<ApiResponse<SystemOverviewResponse>> GetSystemOverviewAsync()
    {
        var overview = await unitOfWork.AggSystemOverviewRepository.GetCurrentAsync();
        if (overview == null)
            return ApiResponse<SystemOverviewResponse>.SuccessResponse(new SystemOverviewResponse(), "Chưa có dữ liệu thống kê");

        return ApiResponse<SystemOverviewResponse>.SuccessResponse(overview.ToResponse(), "Lấy tổng quan hệ thống thành công");
    }
}
