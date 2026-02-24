using Beyond8.Common.Clients;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Subscriptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beyond8.Sale.Application.Clients.Identity;

public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<List<SubscriptionPlanDto>>> GetSubscriptionPlansAsync();
}
