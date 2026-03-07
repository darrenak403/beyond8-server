using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Users;

namespace Beyond8.Learning.Application.Clients.Identity;

public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<UserSimpleResponse>> GetUserByIdAsync(Guid userId);
}
