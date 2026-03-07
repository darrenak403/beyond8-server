using Beyond8.Analytic.Application.Dtos.Stats;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Clients.Learning;

public interface ILearningClient : IBaseClient
{
    Task<ApiResponse<PlatformEnrollmentStatsResponse>> GetPlatformEnrollmentStatsAsync();
}
