using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(PaginationRequest request);
    Task<ApiResponse<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ApiResponse<bool>> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    Task<ApiResponse<string>> UploadUserAvatarAsync(Guid id, UpdateFileUrlRequest request);
    Task<ApiResponse<string>> UploadUserCoverAsync(Guid id, UpdateFileUrlRequest request);

}
