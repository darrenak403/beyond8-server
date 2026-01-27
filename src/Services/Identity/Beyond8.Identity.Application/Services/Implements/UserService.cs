using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements
{
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
                var user = await unitOfWork.UserRepository.AsQueryable()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", id);
                    return ApiResponse<UserResponse>.FailureResponse("Không tìm thấy tài khoản.");
                }

                var response = user.ToUserResponse();
                var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(user.Id);
                response.Subscription = subscription?.ToSubscriptionResponse();
                return ApiResponse<UserResponse>.SuccessResponse(response, "Lấy thông tin tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user by ID {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin tài khoản.");
            }
        }

        public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(PaginationUserRequest request)
        {
            try
            {
                var users = await unitOfWork.UserRepository.SearchUsersPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.Email,
                    request.FullName,
                    request.PhoneNumber,
                    request.Specialization,
                    request.Address,
                    request.IsEmailVerified,
                    request.Role,
                    request.IsDescending ?? true);

                var userResponses = users.Items.Select(u => u.ToUserResponse()).ToList();

                logger.LogInformation("Retrieved {Count} users on page {PageNumber}", userResponses.Count, request.PageNumber);

                return ApiResponse<List<UserResponse>>.SuccessPagedResponse(
                    userResponses,
                    users.TotalCount,
                    request.PageNumber,
                    request.PageSize,
                    "Lấy danh sách tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting all users");
                return ApiResponse<List<UserResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài khoản.");
            }
        }

        public async Task<ApiResponse<UserResponse>> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var normalizedEmail = request.Email.ToLower().Trim();
                var (isEmailValid, emailError) = await ValidateEmailUniqueAsync(normalizedEmail);
                if (!isEmailValid) return ApiResponse<UserResponse>.FailureResponse(emailError!);

                var newUser = request.ToUserEntity();
                newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);

                // Assign roles from request
                if (request.Roles != null && request.Roles.Count != 0)
                {
                    foreach (var roleCode in request.Roles)
                    {
                        var role = await unitOfWork.RoleRepository.FindByCodeAsync(roleCode);
                        if (role != null)
                        {
                            newUser.UserRoles.Add(new UserRole
                            {
                                UserId = newUser.Id,
                                RoleId = role.Id,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            logger.LogWarning("Role with code {RoleCode} not found when creating user", roleCode);
                        }
                    }
                }

                await unitOfWork.UserRepository.AddAsync(newUser);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("User created successfully with email {Email} and ID: {UserId}", normalizedEmail, newUser.Id);

                // Load user with roles for response
                var userWithRoles = await unitOfWork.UserRepository.AsQueryable()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == newUser.Id);
                return ApiResponse<UserResponse>.SuccessResponse(userWithRoles!.ToUserResponse(), "Tạo tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user with email {Email}", request.Email);
                return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi tạo tài khoản.");
            }
        }

        public async Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(id, requireActive: true);
                if (!isValid) return ApiResponse<UserResponse>.FailureResponse(error!);

                user!.UpdateFromRequest(request, currentUserService.UserId);

                await unitOfWork.UserRepository.UpdateAsync(user!.Id, user!);
                await unitOfWork.SaveChangesAsync();

                var updateResponse = user.ToUserResponse();
                var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(user.Id);
                updateResponse.Subscription = subscription?.ToSubscriptionResponse();
                logger.LogInformation("User with ID: {UserId} updated successfully", id);
                return ApiResponse<UserResponse>.SuccessResponse(updateResponse, "Cập nhật tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật tài khoản.");
            }
        }

        public async Task<ApiResponse<UserResponse>> UpdateUserForAdminAsync(Guid id, UpdateUserForAdminRequest request)
        {
            try
            {
                var user = await unitOfWork.UserRepository.AsQueryable()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", id);
                    return ApiResponse<UserResponse>.FailureResponse("Không tìm thấy tài khoản.");
                }

                if (user.Status == UserStatus.Inactive)
                {
                    logger.LogWarning("User with ID: {UserId} is inactive", id);
                    return ApiResponse<UserResponse>.FailureResponse("Tài khoản không hoạt động.");
                }

                await UpdateUserRolesAsync(user, request.Roles);

                await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
                await unitOfWork.SaveChangesAsync();

                var adminResponse = user.ToUserResponse();
                var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(user.Id);
                adminResponse.Subscription = subscription?.ToSubscriptionResponse();
                logger.LogInformation("User roles updated successfully for user with ID: {UserId} by admin/staff", id);
                return ApiResponse<UserResponse>.SuccessResponse(adminResponse, "Thêm vai trò tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user roles for user with ID {UserId}", id);
                return ApiResponse<UserResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật vai trò tài khoản.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(id, requireActive: true);
                if (!isValid) return ApiResponse<bool>.FailureResponse(error!);

                user!.Status = UserStatus.Inactive;

                await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("User with ID: {UserId} deleted (marked as inactive) successfully", id);
                return ApiResponse<bool>.SuccessResponse(true, "Xóa tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa tài khoản.");
            }
        }

        public async Task<ApiResponse<bool>> ToggleUserStatusAsync(Guid id)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(id);
                if (!isValid) return ApiResponse<bool>.FailureResponse(error!);
                var oldStatus = user!.Status;
                switch (user.Status)
                {
                    case UserStatus.Active:
                        user.Status = UserStatus.Inactive;
                        break;
                    case UserStatus.Inactive:
                        user.Status = UserStatus.Active;
                        break;
                    default:
                        logger.LogWarning("Attempted to toggle user {UserId} with restricted status: {Status}", id, user.Status);
                        return ApiResponse<bool>.FailureResponse($"Không thể thay đổi nhanh trạng thái từ '{user.Status}'. Vui lòng cập nhật thủ công.");
                }
                await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("User status updated from {OldStatus} to {NewStatus} for user with ID: {UserId}", oldStatus, user.Status, id);
                return ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái tài khoản thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user status for user with ID {UserId}", id);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật trạng thái tài khoản.");
            }
        }

        public async Task<ApiResponse<string>> UploadUserAvatarAsync(Guid id, UpdateFileUrlRequest request)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(id);
                if (!isValid) return ApiResponse<string>.FailureResponse(error!);

                user!.AvatarUrl = request.FileUrl;

                await unitOfWork.UserRepository.UpdateAsync(user.Id, user!);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("User with ID: {UserId} updated avatar successfully", id);
                return ApiResponse<string>.SuccessResponse(user.AvatarUrl!, "Cập nhật ảnh đại diện thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading avatar for user with ID {UserId}", id);
                return ApiResponse<string>.FailureResponse("Đã xảy ra lỗi khi tải lên ảnh đại diện.");
            }
        }

        public async Task<ApiResponse<string>> UploadUserCoverAsync(Guid id, UpdateFileUrlRequest request)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(id);
                if (!isValid) return ApiResponse<string>.FailureResponse(error!);

                user!.CoverUrl = request.FileUrl;

                await unitOfWork.UserRepository.UpdateAsync(user.Id, user!);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("User with ID: {UserId} updated cover successfully", id);
                return ApiResponse<string>.SuccessResponse(user.CoverUrl!, "Cập nhật ảnh bìa thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading cover for user with ID {UserId}", id);
                return ApiResponse<string>.FailureResponse("Đã xảy ra lỗi khi tải lên ảnh bìa.");
            }
        }

        private async Task<(bool IsValid, string? ErrorMessage, User? ValidUser)> ValidateUserByIdAsync(
            Guid userId,
            bool requireActive = false)
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User not found with ID: {UserId}", userId);
                return (false, "Không tìm thấy tài khoản.", null);
            }

            if (requireActive && user.Status == UserStatus.Inactive)
            {
                logger.LogWarning("User with ID: {UserId} is inactive", userId);
                return (false, "Tài khoản không hoạt động.", user);
            }

            return (true, null, user);
        }


        private async Task<(bool IsValid, string? ErrorMessage)> ValidateEmailUniqueAsync(string email, Guid? excludeUserId = null)
        {
            var existingUser = await unitOfWork.UserRepository.FindOneAsync(u =>
                u.Email == email && (excludeUserId == null || u.Id != excludeUserId));
            if (existingUser != null)
            {
                logger.LogWarning("Email {Email} is already in use", email);
                return (false, "Email này đã được sử dụng.");
            }

            return (true, null);
        }

        private async Task UpdateUserRolesAsync(User user, List<string> roleCodes)
        {
            foreach (var userRole in user.UserRoles.Where(ur => ur.RevokedAt == null))
            {
                userRole.RevokedAt = DateTime.UtcNow;
            }

            if (roleCodes != null && roleCodes.Count != 0)
            {
                foreach (var roleCode in roleCodes)
                {
                    var role = await unitOfWork.RoleRepository.FindByCodeAsync(roleCode);
                    if (role != null)
                    {
                        var existingUserRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
                        if (existingUserRole == null)
                        {
                            user.UserRoles.Add(new UserRole
                            {
                                UserId = user.Id,
                                RoleId = role.Id,
                                AssignedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            existingUserRole.RevokedAt = null;
                            existingUserRole.AssignedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        logger.LogWarning("Role with code {RoleCode} not found when updating user roles", roleCode);
                    }
                }
            }
        }

        public async Task<ApiResponse<SubscriptionResponse>> GetMySubscriptionStatsAsync(Guid userId)
        {
            try
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    logger.LogWarning("User not found with ID: {UserId}", userId);
                    return ApiResponse<SubscriptionResponse>.FailureResponse("Không tìm thấy tài khoản.");
                }

                var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(userId);

                if (subscription == null)
                {
                    logger.LogWarning("User has no active subscription with ID: {UserId}", userId);
                    return ApiResponse<SubscriptionResponse>.FailureResponse("Người dùng không có gói đăng ký.");
                }

                var subscriptionResponse = subscription.ToSubscriptionResponse();

                return ApiResponse<SubscriptionResponse>.SuccessResponse(subscriptionResponse, "Lấy thông tin gói đăng ký thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting my quota for user with ID {UserId}", userId);
                return ApiResponse<SubscriptionResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin gói đăng ký.");
            }
        }

        public async Task<ApiResponse<SubscriptionResponse>> UpdateMySubscriptionAsync(Guid userId, UpdateSubscriptionRequest request)
        {
            try
            {
                var (isValid, error, user) = await ValidateUserByIdAsync(userId, requireActive: true);
                if (!isValid) return ApiResponse<SubscriptionResponse>.FailureResponse(error!);

                var subscription = await unitOfWork.UserSubscriptionRepository.GetActiveByUserIdAsync(userId);

                if (subscription == null)
                {
                    logger.LogWarning("User has no active subscription with ID: {UserId}", userId);
                    return ApiResponse<SubscriptionResponse>.FailureResponse("Người dùng không có gói đăng ký.");
                }

                if (subscription.TotalRemainingRequests < request.NumberOfRequests)
                {
                    return ApiResponse<SubscriptionResponse>.FailureResponse("Số lượng yêu cầu vượt quá số lượng gói đăng ký.");
                }

                subscription.UpdateSubscriptionRequest(request);

                await unitOfWork.UserSubscriptionRepository.UpdateAsync(subscription.Id, subscription);
                await unitOfWork.SaveChangesAsync();

                var subscriptionResponse = subscription.ToSubscriptionResponse();
                return ApiResponse<SubscriptionResponse>.SuccessResponse(subscriptionResponse, "Cập nhật gói đăng ký thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating my subscription for user with ID {UserId}", userId);
                return ApiResponse<SubscriptionResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật gói đăng ký.");
            }
        }
    }
}
