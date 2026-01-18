using System;
using Beyond8.Common.Caching;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Mappings.UserMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class UserService(
    ILogger<UserService> logger,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    PasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<ApiResponse<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            logger.LogInformation("Creating new user with email: {Email}", request.Email);
            var existingUser = await unitOfWork.UserRepository.FindOneAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                logger.LogWarning("User with email {Email} already exists", request.Email);
                return ApiResponse<UserResponse>.FailureResponse("Người dùng với email này đã tồn tại.");
            }

            var user = request.ToEntity(passwordHasher);
            await unitOfWork.UserRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("User created successfully with ID {UserId}", user.Id);

            return ApiResponse<UserResponse>.SuccessResponse(
                user.ToUserResponse(),
                "Tạo người dùng thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return ApiResponse<UserResponse>.FailureResponse("Lỗi khi tạo người dùng.");
        }

    }

    public Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<UserResponse>>> GetUsersAsync(PaginationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        throw new NotImplementedException();
    }
}
