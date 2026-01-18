using System;
using Beyond8.Common.Caching;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit.Internals;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class AuthService(ILogger<AuthService> logger, IUnitOfWork unitOfWork, ITokenService tokenService, PasswordHasher<User> passwordHasher, ICacheService cacheService) : IAuthService
{
    public async Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (user == null)
            {
                logger.LogError("User with email {Email} not found", request.Email);
                return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng, vui lòng thử lại.");
            }
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                logger.LogError("Invalid password for user with email {Email}", request.Email);
                return ApiResponse<TokenResponse>.FailureResponse("Email hoặc mật khẩu không đúng, vui lòng thử lại.");
            }
            if (user.Status != UserStatus.Active)
            {
                logger.LogError("User with email {Email} has status {Status}", request.Email, user.Status);
                return ApiResponse<TokenResponse>.FailureResponse("Tài khoản của bạn không hoạt động, vui lòng liên hệ quản trị viên.");
            }

            var tokenResponse = tokenService.GenerateTokens(user.ToTokenClaims());

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiresAt = tokenResponse.ExpiresAt.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
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
            var existingUser = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (existingUser is not null)
            {
                logger.LogError("User with email {Email} already exists", request.Email);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Email đã được đăng ký, vui lòng thử lại với email khác.");
            }

            var otpCode = GetOtpCode();
            logger.LogError("Send OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            await cacheService.SetAsync($"otp_register:{request.Email}", otpCode, TimeSpan.FromMinutes(5));

            var newUser = request.ToEntity(passwordHasher);
            newUser.Status = UserStatus.Inactive;

            await unitOfWork.UserRepository.AddAsync(newUser);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("User registered successfully: {Email}", request.Email);
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
            if (user == null)
            {
                logger.LogError("User with ID {UserId} not found", userId);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("User with ID {UserId} logged out successfully", userId);
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
            if (user == null)
            {
                logger.LogError("User with ID {UserId} not found", userId);
                return ApiResponse<TokenResponse>.FailureResponse("Người dùng không tồn tại.");
            }
            if (user.Status != UserStatus.Active)
            {
                logger.LogError("User with ID {UserId} has status {Status}", userId, user.Status);
                return ApiResponse<TokenResponse>.FailureResponse("Tài khoản của bạn không hoạt động, vui lòng liên hệ quản trị viên.");
            }

            if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != refreshToken)
            {
                logger.LogError("Invalid refresh token for user with ID {UserId}", userId);
                return ApiResponse<TokenResponse>.FailureResponse("Token làm mới không hợp lệ, vui lòng đăng nhập lại.");
            }
            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                logger.LogError("Expired refresh token for user with ID {UserId}", userId);
                return ApiResponse<TokenResponse>.FailureResponse("Token làm mới đã hết hạn, vui lòng đăng nhập lại.");
            }

            var tokenResponse = tokenService.GenerateTokens(user.ToTokenClaims());

            var timeUntilExpiry = user.RefreshTokenExpiresAt.Value - DateTime.UtcNow;

            var shouldRotateRefreshToken = timeUntilExpiry.TotalDays < 2;

            if (shouldRotateRefreshToken)
            {
                user.RefreshToken = tokenResponse.RefreshToken;
                user.RefreshTokenExpiresAt = tokenResponse.ExpiresAt.AddDays(7);
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = user.Id;

                await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
                await unitOfWork.SaveChangesAsync();
            }
            else
            {
                tokenResponse.RefreshToken = user.RefreshToken;

                logger.LogInformation("Access token refreshed for user: {UserId}, refresh token still valid for {Days} days",
                             userId, timeUntilExpiry.TotalDays);
            }

            logger.LogInformation("Refresh token for user with ID {UserId} successfully", userId);
            return ApiResponse<TokenResponse>.SuccessResponse(tokenResponse, "Làm mới token thành công.");
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
            if (user == null)
            {
                logger.LogError("User with ID {UserId} not found", userId);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Người dùng không tồn tại.");
            }
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                logger.LogError("Invalid old password for user with ID {UserId}", userId);
                return ApiResponse<UserSimpleResponse>.FailureResponse("Mật khẩu cũ không đúng, vui lòng thử lại.");
            }
            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Password changed successfully for user with ID {UserId}", userId);
            return ApiResponse<UserSimpleResponse>.SuccessResponse(user.ToUserSimpleResponse(), "Đổi mật khẩu thành công.");
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
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (user == null)
            {
                logger.LogError("User with email {Email} not found", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.Status != UserStatus.Active)
            {
                logger.LogError("User with email {Email} has status {Status}", request.Email, user.Status);
                return ApiResponse<bool>.FailureResponse("Tài khoản của bạn không hoạt động, vui lòng liên hệ quản trị viên.");
            }

            var otpCode = GetOtpCode();
            logger.LogInformation("Send OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);

            var cacheKey = $"otp_forgot_password:{request.Email}";
            await cacheService.SetAsync(cacheKey, otpCode, TimeSpan.FromMinutes(5));
            return ApiResponse<bool>.SuccessResponse(true, "Mã OTP đã được gửi đến email của bạn.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in forgot password for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Yêu cầu quên mật khẩu thất bại, vui lòng thử lại.");
        }
    }

    public async Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request)
    {
        try
        {
            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);

            if (user == null)
            {
                logger.LogError("User with email {Email} not found", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.Status == UserStatus.Active)
            {
                logger.LogError("User with email {Email} is already active", request.Email);
                return ApiResponse<bool>.FailureResponse("Tài khoản đã được kích hoạt trước đó.");
            }

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

            logger.LogInformation("Resend registration OTP to email: {Email}, OTP: {OtpCode}", request.Email, otpCode);
            logger.LogInformation("Registration OTP resent successfully for user: {Email}", request.Email);
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
            var cachedOtp = await cacheService.GetAsync<string>(cacheKey);
            if (string.IsNullOrEmpty(cachedOtp))
            {
                logger.LogError("OTP for email {Email} not found or expired", request.Email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không hợp lệ hoặc đã hết hạn, vui lòng thử lại.");
            }

            if (cachedOtp != request.OtpCode)
            {
                logger.LogError("Invalid OTP for email {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không đúng, vui lòng thử lại.");
            }

            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (user == null)
            {
                logger.LogError("User with email {Email} not found during password reset", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
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
            var cachedOtp = await cacheService.GetAsync<string>(cacheKey);
            if (string.IsNullOrEmpty(cachedOtp))
            {
                logger.LogError("OTP for email {Email} not found or expired", request.Email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không hợp lệ hoặc đã hết hạn, vui lòng thử lại.");
            }

            if (cachedOtp != request.OtpCode)
            {
                logger.LogError("Invalid OTP for email {Email}", request.Email);
                return ApiResponse<bool>.FailureResponse("Mã OTP không đúng, vui lòng thử lại.");
            }

            var user = await unitOfWork.UserRepository.FindOneAsync(x => x.Email == request.Email);
            if (user == null)
            {
                logger.LogError("User with email {Email} not found during OTP verification", request.Email);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            if (user.Status == UserStatus.Active)
            {
                logger.LogError("User with email {Email} is already active", request.Email);
                return ApiResponse<bool>.FailureResponse("Tài khoản đã được kích hoạt trước đó.");
            }

            user.Status = UserStatus.Active;
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Id;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.SaveChangesAsync();

            await cacheService.RemoveAsync(cacheKey);

            logger.LogInformation("User with email {Email} verified successfully", request.Email);
            return ApiResponse<bool>.SuccessResponse(true, "Xác thực OTP thành công. Tài khoản của bạn đã được kích hoạt.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying registration OTP for email {Email}", request.Email);
            return ApiResponse<bool>.FailureResponse("Xác thực OTP thất bại, vui lòng thử lại.");
        }
    }

    private string GetOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
