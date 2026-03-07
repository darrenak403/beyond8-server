using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Clients.Sale;

public class SaleClient(
    HttpClient httpClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SaleClient> logger)
    : BaseClient(httpClient, httpContextAccessor), ISaleClient
{
    public async Task<ApiResponse<List<Guid>>> GetPurchasedCourseIdsAsync()
    {
        try
        {
            var data = await GetAsync<List<Guid>>("/api/v1/orders/purchased-course-ids");
            return ApiResponse<List<Guid>>.SuccessResponse(data, "Lấy danh sách ID khóa học đã mua thành công");
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get purchased course IDs from Sale service");
            return ApiResponse<List<Guid>>.FailureResponse("Không thể lấy danh sách khóa học đã mua");
        }
    }
}