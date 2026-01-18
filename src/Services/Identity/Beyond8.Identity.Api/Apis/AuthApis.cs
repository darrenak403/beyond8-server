using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Identity.Api.Apis;

public static class AuthApis
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/auth")
            .MapAuthRoutes()
            .WithTags("Authentication Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapAuthRoutes(this RouteGroupBuilder group)
    {
        // group.MapPost("/login", LoginUserAsync)
        //     .WithName("Login")
        //     .WithDescription("Đăng nhập người dùng")
        //     .AllowAnonymous()
        //     .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", RegisterUserAsync)
            .WithName("Register")
            .WithDescription("Đăng ký người dùng mới")
            .AllowAnonymous()
            .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest);

        // group.MapPost("/refresh-token", RefreshTokenAsync)
        //     .WithName("RefreshToken")
        //     .WithDescription("Làm mới access token")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest)
        //     .Produces(StatusCodes.Status401Unauthorized);

        // group.MapPut("/change-password", ChangePasswordAsync)
        //     .WithName("ChangePassword")
        //     .WithDescription("Đổi mật khẩu người dùng")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status400BadRequest)
        //     .Produces(StatusCodes.Status401Unauthorized);

        // group.MapPost("/verify-otp", VerifyOtpAsync)
        //     .WithName("VerifyOtp")
        //     .WithDescription("Xác thực OTP đăng ký")
        //     .AllowAnonymous()
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // group.MapPost("/resend-otp", ResendOtpAsync)
        //     .WithName("ResendOtp")
        //     .WithDescription("Gửi lại mã OTP (Giới hạn 60s/lần)")
        //     .AllowAnonymous()
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // group.MapPost("/forgot-password", ForgotPasswordAsync)
        //     .WithName("ForgotPassword")
        //     .WithDescription("Yêu cầu mã OTP lấy lại mật khẩu")
        //     .AllowAnonymous()
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        // group.MapPost("/reset-password", ResetPasswordAsync)
        //     .WithName("ResetPassword")
        //     .WithDescription("Đặt lại mật khẩu người dùng")
        //     .AllowAnonymous()
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // group.MapPost("/logout", LogoutUserAsync)
        //     .WithName("Logout")
        //     .WithDescription("Đăng xuất người dùng")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
        //     .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> RegisterUserAsync(
        [FromBody] RegisterRequest request,
        [FromServices] IAuthService authService)
    {
        var result = await authService.RegisterUserAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Message);
    }
}
