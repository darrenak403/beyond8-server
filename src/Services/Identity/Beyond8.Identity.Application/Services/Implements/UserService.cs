using Beyond8.Common.Caching;
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
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tìm thấy tài khoản với ID: {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Không tìm thấy tài khoản.");
            }

            return ApiResponse<UserResponse>.SuccessResponse(user.ToUserResponse(), "Lấy thông tin tài khoản thành công.");
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
                pageSize: request.PageSize
            );
            var userResponses = users.Items.Select(u => u.ToUserResponse()).ToList();

            logger.LogInformation("Lấy được {Count} tài khoản trên trang {PageNumber}", userResponses.Count, request.PageNumber);

            return ApiResponse<List<UserResponse>>.SuccessPagedResponse(
                userResponses,
                userResponses.Count,
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
            var existingUser = await unitOfWork.UserRepository.FindOneAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                logger.LogWarning("Tài khoản với email {Email} đã tồn tại.", request.Email);
                return ApiResponse<UserResponse>.FailureResponse("Email này đã được sử dụng.");
            }
            var newUser = request.ToCreateUserEntity();

            await unitOfWork.UserRepository.AddAsync(newUser);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Tài khoản với email {Email} đã được tạo thành công.", request.Email);
            logger.LogInformation("Tài khoản với ID: {UserId}", newUser.Id);

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
            var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tìm thấy tài khoản với ID: {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Không tìm thấy tài khoản.");
            }

            if (user.Status == UserStatus.Inactive)
            {
                logger.LogWarning("Không thể cập nhật tài khoản không hoạt động. ID: {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Không thể cập nhật tài khoản không hoạt động.");
            }

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var emailExists = await unitOfWork.UserRepository.FindOneAsync(u => u.Email == request.Email);
                if (emailExists != null)
                {
                    logger.LogWarning("Email {Email} đã được sử dụng bởi tài khoản khác.", request.Email);
                    return ApiResponse<UserResponse>.FailureResponse("Email này đã được sử dụng.");
                }
                user.Email = request.Email;
            }

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
            var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tìm thấy tài khoản với ID: {UserId}", id);
                return ApiResponse<bool>.FailureResponse("Không tìm thấy tài khoản.");
            }
            if (user.Status == UserStatus.Inactive)
            {
                logger.LogWarning("Chỉ có thể xóa tài khoản không hoạt động. ID: {UserId}", id);
                return ApiResponse<bool>.FailureResponse("Chỉ có thể xóa tài khoản không hoạt động.");
            }

            user.Status = UserStatus.Inactive;
            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Tài khoản với ID: {UserId} đã được xóa thành công.", id);
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
            var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tìm thấy tài khoản với ID: {UserId}", id);
                return ApiResponse<bool>.FailureResponse("Không tìm thấy tài khoản.");
            }

            if (user.Status == request.NewStatus)
            {
                logger.LogWarning("Tài khoản với ID: {UserId} đã có trạng thái {Status}.", id, request.NewStatus);
                return ApiResponse<bool>.FailureResponse("Tài khoản đã có trạng thái này.");
            }

            var oldStatus = user.Status;
            user.Status = request.NewStatus;
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
}
