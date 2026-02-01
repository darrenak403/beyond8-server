using Beyond8.Catalog.Application.Dtos.Users;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Clients.Identity;

public class IdentityClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<IdentityClient> logger) : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
    public async Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid userId)
    {
        try
        {
            var data = await GetAsync<bool>($"/api/v1/instructors/{userId}/verified");
            return ApiResponse<bool>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CheckInstructorProfileVerifiedAsync failed for user {UserId}", userId);
            return ApiResponse<bool>.FailureResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<UserSimpleResponse>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var data = await GetAsync<UserSimpleResponse>($"/api/v1/users/{userId}");
            return ApiResponse<UserSimpleResponse>.SuccessResponse(data, "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetUserByIdAsync failed for user {UserId}", userId);
            return ApiResponse<UserSimpleResponse>.FailureResponse(ex.Message);
        }
    }
}
