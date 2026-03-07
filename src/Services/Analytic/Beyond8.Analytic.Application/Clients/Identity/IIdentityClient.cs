using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Clients.Identity;

public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<PlatformUserStatsResponse>> GetPlatformUserStatsAsync();
}
