using System;
using Beyond8.Common.Caching;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit.Internals;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class AuthService(ILogger<AuthService> logger, IUnitOfWork unitOfWork, ITokenService tokenService, PasswordHasher<User> passwordHasher, ICacheService cacheService) : IAuthService
{
    public Task<ApiResponse<UserSimpleResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> LogoutUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken)
    {
        throw new NotImplementedException();
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

            var newUser = request.ToEntity(passwordHasher);

            var otpCode = GetOtpCode();
            logger.LogError("Send OTP code to email: {Email} with OTP: {OtpCode}", request.Email, otpCode);
            await cacheService.SetAsync($"otp_register:{request.Email}", otpCode, TimeSpan.FromMinutes(5));

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

    public Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request)
    {
        throw new NotImplementedException();
    }

    private string GetOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
