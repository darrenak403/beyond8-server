using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<UserSimpleResponse>> RegisterUserAsync(RegisterRequest request);
    Task<ApiResponse<TokenResponse>> LoginUserAsync(LoginRequest request);
    Task<ApiResponse<TokenResponse>> RefreshTokenAsync(Guid userId, string refreshToken);
    Task<ApiResponse<UserSimpleResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<ApiResponse<bool>> VerifyRegisterOtpAsync(VerifyOtpRequest request);
    Task<ApiResponse<bool>> ResendRegisterOtpAsync(ResendOtpRequest request);
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse<bool>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequest request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse<bool>> LogoutUserAsync(Guid userId);
}
