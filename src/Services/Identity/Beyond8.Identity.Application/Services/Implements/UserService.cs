using Beyond8.Common.Caching;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class UserService(
    ILogger<UserService> logger,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    PasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var (isValid, error, user) = await ValidateUserByIdAsync(id);
            if (!isValid) return ApiResponse<UserResponse>.FailureResponse(error!);

            return ApiResponse<UserResponse>.SuccessResponse(user!.ToUserResponse(), "Lấy thông tin tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi lấy thông tin tài khoản với ID {UserId}", id);
            return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin tài khoản.");
        }
    }

    public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(PaginationRequest request)
    {
        try
        {
            var users = await unitOfWork.UserRepository.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                filter: null,
                orderBy: query => query.OrderBy(u => u.CreatedAt)
            );
            var userResponses = users.Items.Select(u => u.ToUserResponse()).ToList();

            logger.LogInformation("Lấy được {Count} tài khoản trên trang {PageNumber}", userResponses.Count, request.PageNumber);

            return ApiResponse<List<UserResponse>>.SuccessPagedResponse(
                userResponses,
                users.TotalCount,
                request.PageNumber,
                request.PageSize,
                "Lấy danh sách tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi lấy danh sách tài khoản.");
            return ApiResponse<List<UserResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài khoản.");
        }
    }

    public async Task<ApiResponse<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var (isEmailValid, emailError) = await ValidateEmailUniqueAsync(request.Email);
            if (!isEmailValid) return ApiResponse<UserResponse>.FailureResponse(emailError!);

            var newUser = request.ToUserEntity(currentUserService.UserId);
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);

            await unitOfWork.UserRepository.AddAsync(newUser);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Tài khoản với email {Email} đã được tạo thành công với ID: {UserId}", request.Email, newUser.Id);

            return ApiResponse<UserResponse>.SuccessResponse(newUser.ToUserResponse(), "Tạo tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi tạo tài khoản với email {Email}", request.Email);
            return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi tạo tài khoản.");
        }
    }

    public async Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var (isValid, error, user) = await ValidateUserByIdAsync(id, requireActive: true);
            if (!isValid) return ApiResponse<UserResponse>.FailureResponse(error!);

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user!.Email)
            {
                var (isEmailValid, emailError) = await ValidateEmailUniqueAsync(request.Email, user.Id);
                if (!isEmailValid) return ApiResponse<UserResponse>.FailureResponse(emailError!);
                user.IsEmailVerified = false;
            }

            user.UpdateFromRequest(request, currentUserService.UserId);

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Tài khoản với ID: {UserId} đã được cập nhật thành công.", id);
            return ApiResponse<UserResponse>.SuccessResponse(user.ToUserResponse(), "Cập nhật tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi cập nhật tài khoản với ID {UserId}", id);
            return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật tài khoản.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        try
        {
            var (isValid, error, user) = await ValidateUserByIdAsync(id, requireActive: true);
            if (!isValid) return ApiResponse<bool>.FailureResponse(error!);

            user!.Status = UserStatus.Inactive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserService.UserId;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Tài khoản với ID: {UserId} đã được xóa (đánh dấu không hoạt động) thành công.", id);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi xóa tài khoản với ID {UserId}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa tài khoản.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request)
    {
        try
        {
            var (isValid, error, user) = await ValidateUserByIdAsync(id);
            if (!isValid) return ApiResponse<bool>.FailureResponse(error!);

            if (user!.Status == request.NewStatus)
            {
                logger.LogWarning("Tài khoản với ID: {UserId} đã có trạng thái {Status}.", id, request.NewStatus);
                return ApiResponse<bool>.FailureResponse("Tài khoản đã có trạng thái này.");
            }

            var oldStatus = user.Status;
            user.Status = request.NewStatus;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserService.UserId;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Trạng thái tài khoản với ID: {UserId} đã được cập nhật từ {OldStatus} sang {NewStatus}.", id, oldStatus, request.NewStatus);
            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái tài khoản thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đã xảy ra lỗi khi cập nhật trạng thái tài khoản với ID {UserId}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật trạng thái tài khoản.");
        }
    }

    /// <summary>
    /// Validates user by Id. Returns (IsValid, ErrorMessage, ValidUser). Use for GetUserById, UpdateUser, DeleteUser, UpdateUserStatus.
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage, User? ValidUser)> ValidateUserByIdAsync(
        Guid userId,
        bool requireActive = false)
    {
        var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == userId);
        if (user == null)
        {
            logger.LogWarning("Không tìm thấy tài khoản với ID: {UserId}", userId);
            return (false, "Không tìm thấy tài khoản.", null);
        }

        if (requireActive && user.Status == UserStatus.Inactive)
        {
            logger.LogWarning("Tài khoản với ID: {UserId} không hoạt động.", userId);
            return (false, "Tài khoản không hoạt động.", user);
        }

        return (true, null, user);
    }

    /// <summary>
    /// Validates email uniqueness. Returns (IsValid, ErrorMessage).
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        var existingUser = await unitOfWork.UserRepository.FindOneAsync(u =>
            u.Email == email && (excludeUserId == null || u.Id != excludeUserId));
        if (existingUser != null)
        {
            logger.LogWarning("Email {Email} đã được sử dụng.", email);
            return (false, "Email này đã được sử dụng.");
        }

        return (true, null);
    }
}
