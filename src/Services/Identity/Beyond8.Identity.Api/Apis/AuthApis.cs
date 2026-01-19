using System;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Auth;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Services.Interfaces;
using FluentValidation;
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
        group.MapPost("/login", LoginUserAsync)
            .WithName("Login")
            .WithDescription("Đăng nhập người dùng (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", RegisterUserAsync)
            .WithName("Register")
            .WithDescription("Đăng ký người dùng mới (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh-token", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithDescription("Làm mới access token (Cần xác thực)")
            .RequireAuthorization()
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithDescription("Đổi mật khẩu người dùng (Cần xác thực)")
            .RequireAuthorization()
            .Produces<ApiResponse<UserSimpleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("register/verify-otp", VerifyOtpAsync)
            .WithName("VerifyOtp")
            .WithDescription("Xác thực OTP đăng ký (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        group.MapPost("/resend-otp", ResendOtpAsync)
            .WithName("ResendOtp")
            .WithDescription("Gửi lại mã OTP (Giới hạn 60s/lần) (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithDescription("Yêu cầu mã OTP lấy lại mật khẩu (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        group.MapPost("/forgot-password/verify-otp", VerifyForgotPasswordOtpAsync)
            .WithName("VerifyForgotPasswordOtp")
            .WithDescription("Xác thực OTP quên mật khẩu (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithDescription("Đặt lại mật khẩu người dùng (Không cần xác thực)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutUserAsync)
            .WithName("Logout")
            .WithDescription("Đăng xuất người dùng (Cần xác thực)")
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> LoginUserAsync(
        [FromBody] LoginRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<LoginRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.LoginUserAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> LogoutUserAsync(
        [FromServices] IAuthService authService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await authService.LogoutUserAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RegisterUserAsync(
        [FromBody] RegisterRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<RegisterRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.RegisterUserAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RefreshTokenAsync(
    [FromBody] RefreshTokenRequest request,
    [FromServices] IAuthService authService,
    [FromServices] ICurrentUserService currentUserService,
    [FromServices] IValidator<RefreshTokenRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.RefreshTokenAsync(currentUserService.UserId, request.RefreshToken);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);

    }

    private static async Task<IResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request,
        [FromServices] IAuthService authService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<ChangePasswordRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.ChangePasswordAsync(currentUserService.UserId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Message);
    }

    private static async Task<IResult> VerifyOtpAsync(
        [FromBody] VerifyOtpRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<VerifyOtpRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.VerifyRegisterOtpAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ResendOtpAsync(
        [FromBody] ResendOtpRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<ResendOtpRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.ResendRegisterOtpAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }


    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<ResetPasswordRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.ResetPasswordAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<ForgotPasswordRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.ForgotPasswordAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> VerifyForgotPasswordOtpAsync(
        [FromBody] VerifyForgotPasswordOtpRequest request,
        [FromServices] IAuthService authService,
        [FromServices] IValidator<VerifyForgotPasswordOtpRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await authService.VerifyForgotPasswordOtpAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}


