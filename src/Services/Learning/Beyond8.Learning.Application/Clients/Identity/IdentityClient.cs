using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beyond8.Learning.Application.Clients.Identity;

public class IdentityClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<IdentityClient> logger) : BaseClient(httpClient, httpContextAccessor), IIdentityClient
{
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
