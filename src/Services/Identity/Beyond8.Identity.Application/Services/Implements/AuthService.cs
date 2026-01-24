using Beyond8.Common.Caching;
using Beyond8.Common.Events.Identity;
using Beyond8.Common.Utilities;
using MassTransit;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class AuthService(
    ILogger<AuthService> logger,
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    PasswordHasher<User> passwordHasher,
    ICacheService cacheService,
    IPublishEndpoint publishEndpoint) : IAuthService
{
    public async Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request)
    {
        try
        {
            var normalizedEmail = request.Email.ToLower().Trim();
            var user = await unitOfWork.UserRepository.AsQueryable()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
            var validation = ValidateUserByEmail(user, request.Email);
            if (!validation.IsValid)
                return ApiResponse<TokenResponse>.FailureResponse(validation.ErrorMessage!);
            var u = validation.ValidUser!;

            var verifyPasswordResult = passwordHasher.VerifyHashedPassword(u, u.PasswordHash, request.Password);
            if (verifyPasswordResult == PasswordVerificationResult.Failed)
            {
                logger.LogError("Invalid password for user with email {Email}", request.Email);
                return ApiResponse<TokenResponse>.FailureResponse("Mật khẩu không đúng, vui lòng thử lại.");
            }

            var tokenResponse = tokenService.GenerateTokens(u.ToTokenClaims());

            u.RefreshToken = tokenResponse.RefreshToken;
            u.RefreshTokenExpiresAt = tokenResponse.ExpiresAt.AddDays(7);
            u.LastLoginAt = DateTime.UtcNow;
            u.LoginAttempts = 0;
            u.LockedUntil = null;
            u.IsRevoked = false;

            await unitOfWork.UserRepository.UpdateAsync(u.Id, u);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("User with email {Email} logged in successfully", request.Email);
            return ApiResponse<TokenResponse>.SuccessResponse(tokenResponse, "Đăng nhập thành công.");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging in user");
            return ApiResponse<TokenResponse>.FailureResponse("Đăng nhập thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<UserSimpleResponse>> RegisterUserAsync(RegisterRequest request)
    {
        try
        {
            var normalizedEmail = request.Email.ToLower().Trim();
            var existingUser = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == normalizedEmail);
            if (existingUser is not null)
            {
                logger.LogError("User with email {Email} already exists", request.Email);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Email đã được đăng ký, vui lòng thử lại với email khác.");
            }

            var otpCode = GetOtpCode();
            logger.LogInformation("Sending OTP code to email: {Email} with OTP: {OtpCode}", normalizedEmail, otpCode);
            await cacheService.SetAsync($"otp_register:{normalizedEmail}", otpCode, TimeSpan.FromMinutes(5));

            var newUser = request.ToEntity(passwordHasher);

            // Assign default Student role
            var studentRole = await unitOfWork.RoleRepository.FindByCodeAsync("ROLE_STUDENT");
            if (studentRole == null)
            {
                logger.LogError("Student role not found in database");
                return ApiResponse<UserSimpleResponse>.FailureResponse("Hệ thống chưa được cấu hình đúng. Vui lòng liên hệ quản trị viên.");
            }

            newUser.UserRoles.Add(new UserRole
            {
                UserId = newUser.Id,
                RoleId = studentRole.Id,
                AssignedAt = DateTime.UtcNow
            });

            await unitOfWork.UserRepository.AddAsync(newUser);
            await unitOfWork.SaveChangesAsync();

            var otpEvent = new OtpEmailEvent(
                newUser.Id,
                normalizedEmail,
                newUser.FullName,
                otpCode,
                "Đăng ký tài khoản",
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(otpEvent);

            logger.LogInformation("User registered successfully: {Email}", normalizedEmail);
            return ApiResponse<UserSimpleResponse>.SuccessResponse(
                newUser.ToUserSimpleResponse(),
                "Đăng ký người dùng thành công, vui lòng kiểm tra email để xác thực OTP");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering user");
            return ApiResponse<UserSimpleResponse>.FailureResponse("Đăng ký thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> LogoutUserAsync(Guid userId)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Id == userId);
            var validation = ValidateUserById(user, userId);
            if (!validation.IsValid)
                return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);
            var u = validation.ValidUser!;

            u.IsRevoked = true;
            u.RevokedAt = DateTime.UtcNow;

            await unitOfWork.UserRepository.UpdateAsync(u.Id, u);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Đăng xuất thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging out user");
            return ApiResponse<bool>.FailureResponse("Đăng xuất thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Id == userId);

            var validation = ValidateUserById(user, userId, requireActive: true, requireValidRefreshToken: true, refreshToken);
            if (!validation.IsValid)
                return ApiResponse<TokenResponse>.FailureResponse(validation.ErrorMessage!);

            var u = validation.ValidUser!;

            if (u.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponse<TokenResponse>.FailureResponse("Refresh token đã hết hạn. Vui lòng đăng nhập lại.");
            }

            var tokenResponse = tokenService.GenerateTokens(u.ToTokenClaims());

            var timeUntilExpiry = u.RefreshTokenExpiresAt!.Value - DateTime.UtcNow;

            var shouldRotateRefreshToken = timeUntilExpiry.TotalDays < 2;

            if (shouldRotateRefreshToken)
            {
                u.RefreshToken = tokenResponse.RefreshToken;
                u.RefreshTokenExpiresAt = tokenResponse.ExpiresAt.AddDays(7);

                await unitOfWork.UserRepository.UpdateAsync(u.Id, u);
                await unitOfWork.SaveChangesAsync();
            }
            else
            {
                tokenResponse.RefreshToken = u.RefreshToken!;
                logger.LogInformation("Access token refreshed for user: {UserId}, refresh token still valid for {Days} days",
                                userId, timeUntilExpiry.TotalDays);
            }

            logger.LogInformation("Refresh token for user with ID {UserId} successfully", userId);
            return ApiResponse<TokenResponse>.SuccessResponse(tokenResponse, "Làm mới token thành công, vui lòng sử dụng token mới để truy cập vào hệ thống.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token for user with ID {UserId}", userId);
            return ApiResponse<TokenResponse>.FailureResponse("Đã xảy ra lỗi trong quá trình làm mới token. Vui lòng thử lại!");
        }
    }

    public async Task<ApiResponse<UserSimpleResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Id == userId);
            var validation = ValidateUserById(user, userId);
            if (!validation.IsValid)
                return ApiResponse<UserSimpleResponse>.FailureResponse(validation.ErrorMessage!);
            var u = validation.ValidUser!;

            var verifyOldPasswordResult = passwordHasher.VerifyHashedPassword(u, u.PasswordHash, request.OldPassword);
            if (verifyOldPasswordResult == PasswordVerificationResult.Failed)
            {
                logger.LogError("Invalid old password for user with ID {UserId}", userId);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Mật khẩu cũ không đúng, vui lòng thử lại.");
            }
            if (request.OldPassword == request.NewPassword)
            {
                logger.LogError("New password cannot be the same as the old password for user with ID {UserId}", userId);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Mật khẩu mới không được trùng với mật khẩu cũ, vui lòng chọn mật khẩu khác.");
            }

            u.PasswordHash = passwordHasher.HashPassword(u, request.NewPassword);
            await unitOfWork.UserRepository.UpdateAsync(u.Id, u);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Password changed successfully for user with ID {UserId}", userId);
            return ApiResponse<UserSimpleResponse>.SuccessResponse(u.ToUserSimpleResponse(), "Đổi mật khẩu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing password for user with ID {UserId}", userId);
            return ApiResponse<UserSimpleResponse>.FailureResponse("Đổi mật khẩu thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var lockKey = $"otp_forgot_password_lock:{request.Email}";
            var isLocked = await cacheService.GetAsync<bool?>(lockKey);
            if (isLocked == true)
            {
                logger.LogError("Forgot password request is locked for email {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây khi gửi lại mã OTP.");
            }
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            var validation = ValidateUserByEmail(user, request.Email);
            if (!validation.IsValid)
                return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);

            var otpCode = GetOtpCode();
            logger.LogInformation("Sending forgot password OTP to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            var cacheKey = $"otp_forgot_password:{request.Email}";
            await cacheService.SetAsync(cacheKey, otpCode, TimeSpan.FromMinutes(5));
            await cacheService.SetAsync(lockKey, true, TimeSpan.FromSeconds(60));

            var u = validation.ValidUser!;
            var otpEvent = new OtpEmailEvent(
                u.Id,
                request.Email,
                u.FullName,
                otpCode,
                "Quên mật khẩu",
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(otpEvent);

            return ApiResponse<bool>.SuccessResponse(true, "Mã OTP đã được gửi đến email của bạn.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in forgot password for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Yêu cầu quên mật khẩu thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request)
    {
        try
        {
            var cacheKey = $"otp_forgot_password:{request.Email}";
            var otpValidation = await ValidateOtpFromCacheAsync(cacheKey, request.OtpCode, request.Email);
            if (!otpValidation.IsValid)
                return ApiResponse<bool>.FailureResponse(otpValidation.ErrorMessage!);

            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            var validation = ValidateUserByEmail(user, request.Email);
            if (!validation.IsValid)
                return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);

            logger.LogInformation("OTP verified successfully for forgot password: {Email}", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Xác thực OTP thành công. Bạn có thể đặt lại mật khẩu.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying forgot password OTP for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Xác thực OTP thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            var validation = ValidateUserByEmail(user, request.Email, false, false);
            if (!validation.IsValid)
                return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);

            var lockKey = $"otp_resend_lock:{request.Email}";
            var isLocked = await cacheService.GetAsync<bool?>(lockKey);
            if (isLocked == true)
            {
                logger.LogError("Resend OTP request is locked for email {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Vui lòng đợi 60 giây khi gửi lại mã OTP.");
            }

            var otpCode = GetOtpCode();
            await cacheService.SetAsync($"otp_register:{request.Email}", otpCode, TimeSpan.FromMinutes(5));
            await cacheService.SetAsync(lockKey, true, TimeSpan.FromSeconds(60));

            var u = validation.ValidUser!;
            var otpEvent = new OtpEmailEvent(
                u.Id,
                request.Email,
                u.FullName,
                otpCode,
                "Gửi lại OTP đăng ký",
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(otpEvent);

            logger.LogInformation("Resending registration OTP to email: {Email}, OTP: {OtpCode}", request.Email, otpCode);
            return ApiResponse<bool>.SuccessResponse(true, "Mã OTP đã được gửi lại. Vui lòng kiểm tra email của bạn.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resending registration OTP for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Gửi lại mã OTP thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var cacheKey = $"otp_forgot_password:{request.Email}";
            var otpValidation = await ValidateOtpFromCacheAsync(cacheKey, request.OtpCode, request.Email);
            if (!otpValidation.IsValid)
                return ApiResponse<bool>.FailureResponse(otpValidation.ErrorMessage!);

            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            var validation = ValidateUserByEmail(user, request.Email);
            if (!validation.IsValid)
                return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);
            var u = validation.ValidUser!;

            u.PasswordHash = passwordHasher.HashPassword(u, request.NewPassword);
            await unitOfWork.UserRepository.UpdateAsync(u.Id, u);
            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoveAsync(cacheKey);

            logger.LogInformation("Password reset successfully for email {Email}", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Đặt lại mật khẩu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting password for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Đặt lại mật khẩu thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request)
    {
        try
        {
            var cacheKey = $"otp_register:{request.Email}";
            var otpValidation = await ValidateOtpFromCacheAsync(cacheKey, request.OtpCode, request.Email);
            if (!otpValidation.IsValid)
                return ApiResponse<bool>.FailureResponse(otpValidation.ErrorMessage!);

            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (user == null)
            {
                logger.LogError("User with email {Email} not found during OTP verification", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.IsEmailVerified)
            {
                logger.LogError("User with email {Email} is already verified", request.Email);
                return ApiResponse<bool>.FailureResponse("Tài khoản đã được xác thực trước đó.");
            }

            user.IsEmailVerified = true;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoveAsync(cacheKey);

            logger.LogInformation("User with email {Email} verified successfully", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Xác thực OTP thành công. Tài khoản của bạn đã được xác thực.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying registration OTP for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Xác thực OTP thất bại, vui lòng thử lại.");
        }
    }

    /// <summary>
    /// Validates user by Id. Returns (IsValid, ErrorMessage, ValidUser). Use for Logout, ChangePassword, RefreshToken.
    /// </summary>
    private (bool IsValid, string? ErrorMessage, User? ValidUser) ValidateUserById(
        User? user,
        Guid userId,
        bool requireActive = false,
        bool requireValidRefreshToken = false,
        string? refreshToken = null)
    {
        if (user == null)
        {
            logger.LogError("User with ID {UserId} not found", userId);
            return (false, "Người dùng không tồn tại.", null);
        }
        if (requireActive && user.Status != UserStatus.Active)
        {
            logger.LogError("User with ID {UserId} has status {Status}", userId, user.Status);
            return (false, "Tài khoản của bạn không hoạt động, vui lòng liên hệ quản trị viên.", null);
        }
        if (requireValidRefreshToken && !string.IsNullOrEmpty(refreshToken))
        {
            if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != refreshToken)
            {
                logger.LogError("Invalid refresh token for user with ID {UserId}", userId);
                return (false, "Token làm mới không hợp lệ, vui lòng đăng nhập lại.", null);
            }
            if (user.IsRevoked == true)
            {
                logger.LogError("User with ID {UserId} is revoked", userId);
                return (false, "Tài khoản của bạn đã bị đăng xuất, vui lòng đăng nhập lại.", null);
            }
            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                logger.LogError("Expired refresh token for user with ID {UserId}", userId);
                return (false, "Token làm mới đã hết hạn, vui lòng đăng nhập lại.", null);
            }
        }
        return (true, null, user);
    }

    /// <summary>
    /// Validates user by Email. Returns (IsValid, ErrorMessage, ValidUser). Use for Login, ForgotPassword, ResendOtp, ResetPassword.
    /// </summary>
    private (bool IsValid, string? ErrorMessage, User? ValidUser) ValidateUserByEmail(
        User? user,
        string email,
        bool requireActive = true,
        bool requireEmailVerified = true)
    {
        if (user == null)
        {
            logger.LogError("User with email {Email} not found", email);
            return (false, "Người dùng không tồn tại.", null);
        }
        if (requireEmailVerified && !user.IsEmailVerified)
        {
            logger.LogError("User with email {Email} is not verified", email);
            return (false, "Tài khoản của bạn chưa được xác thực, vui lòng kiểm tra email để xác thực.", null);
        }
        if (requireActive && user.Status != UserStatus.Active)
        {
            logger.LogError("User with email {Email} has status {Status}", email, user.Status);
            return (false, "Tài khoản của bạn không hoạt động, vui lòng liên hệ quản trị viên.", null);
        }
        return (true, null, user);
    }

    /// <summary>
    /// Validates OTP from cache. Returns (IsValid, ErrorMessage).
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateOtpFromCacheAsync(string cacheKey, string otpCode, string email)
    {
        var cachedOtp = await cacheService.GetAsync<string>(cacheKey);
        if (string.IsNullOrEmpty(cachedOtp))
        {
            logger.LogError("OTP for email {Email} not found or expired", email);
            return (false, "Mã OTP không hợp lệ hoặc đã hết hạn, vui lòng thử lại.");
        }

        if (cachedOtp != otpCode)
        {
            logger.LogError("Invalid OTP for email {Email}", email);
            return (false, "Mã OTP không đúng, vui lòng thử lại.");
        }

        return (true, null);
    }

    private string GetOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
