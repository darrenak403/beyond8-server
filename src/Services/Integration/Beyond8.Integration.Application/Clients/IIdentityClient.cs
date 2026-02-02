using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Clients.Identity;

namespace Beyond8.Integration.Application.Clients;

public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<SubscriptionResponse>> GetUserSubscriptionAsync(Guid userId);

    Task<ApiResponse<SubscriptionResponse>> UpdateUserSubscriptionAsync(Guid userId, UpdateUsageQuotaRequest request);
}
