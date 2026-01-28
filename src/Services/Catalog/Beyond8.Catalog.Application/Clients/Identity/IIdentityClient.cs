using System;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Clients.Identity;

public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid instructorId);
}
