using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Clients.Sale;

public interface ISaleClient : IBaseClient
{
    Task<ApiResponse<List<Guid>>> GetPurchasedCourseIdsAsync();
}
