using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<List<UserResponse>>> GetUsersAsync(PaginationRequest request);
    Task<ApiResponse<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ApiResponse<bool>> UpdateUserStatusAsync(Guid id, bool isActive);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    // Task<ApiResponse<string>> UploadUserAvatarAsync(Guid id, byte[] avatarData, string fileName);
}
