using System;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Clients.Identity;

public class IdentityClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<IdentityClient> logger) : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
    public async Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid instructorId)
    {
        try
        {
            var data = await GetAsync<bool>($"/api/v1/instructors/{instructorId}/verified");
            return ApiResponse<bool>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CheckInstructorProfileVerifiedAsync failed for instructor {InstructorId}", instructorId);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }
}
