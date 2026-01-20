using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Identity.Api.Apis
{
    public static class UserApis
    {
        public static IEndpointRouteBuilder MapUserApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/users")
                .MapUserRoutes()
                .WithTags("User Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapUserRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllUsersAsync)
                .WithName("GetAllUsers")
                .WithDescription("Lấy danh sách tất cả người dùng")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                .Produces<ApiResponse<List<UserResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<UserResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:guid}", GetUserByIdAsync)
                .WithName("GetUserById")
                .WithDescription("Lấy thông tin người dùng theo ID")
                .RequireAuthorization()
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/", CreateUserAsync)
                .WithName("CreateUser")
                .WithDescription("Tạo tài khoản người dùng mới")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}", UpdateUserAsync)
                .WithName("UpdateUser")
                .WithDescription("Cập nhật thông tin người dùng theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UserResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapDelete("/{id:guid}", DeleteUserAsync)
                .WithName("DeleteUser")
                .WithDescription("Xóa tài khoản người dùng theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapPut("/{id:guid}/status", UpdateUserStatusAsync)
                .WithName("UpdateUserStatus")
                .WithDescription("Cập nhật trạng thái người dùng theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/me", GetMyProfileAsync)
               .WithName("GetMyProfile")
               .WithDescription("Lấy thông tin cá nhân của người dùng hiện tại")
               .RequireAuthorization()
               .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
               .Produces<ApiResponse<UserResponse>>(StatusCodes.Status400BadRequest)
               .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/me", UpdateMyProfileAsync)
               .WithName("UpdateMyProfile")
               .WithDescription("Cập nhật thông tin cá nhân của người dùng hiện tại")
               .RequireAuthorization()
               .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
               .Produces<ApiResponse<UserResponse>>(StatusCodes.Status400BadRequest)
               .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/avatar", UploadUserAvatarAsync)
                .WithName("UploadUserAvatar")
                .WithDescription("Tải lên ảnh đại diện cho người dùng theo ID")
                .RequireAuthorization()
                .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/coverimage", UploadUserCoverAsync)
            .WithName("UploadUserCover")
            .WithDescription("Tải lên ảnh bìa cho người dùng theo ID")
            .RequireAuthorization()
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<string>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> UploadUserCoverAsync(
           [FromServices] ICurrentUserService currentUserService,
           [FromBody] UpdateFileUrlRequest request,
           [FromServices] IUserService userService)
        {
            var response = await userService.UploadUserCoverAsync(currentUserService.UserId, request);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }


        private static async Task<IResult> UploadUserAvatarAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] UpdateFileUrlRequest request,
            [FromServices] IUserService userService)
        {
            var response = await userService.UploadUserAvatarAsync(currentUserService.UserId, request);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }

        private static async Task<IResult> GetMyProfileAsync(
            [FromServices] IUserService userService, [FromServices] ICurrentUserService currentUserService)
        {
            var response = await userService.GetUserByIdAsync(currentUserService.UserId);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.BadRequest(response);
        }

        private static async Task<IResult> UpdateMyProfileAsync(
            [FromBody] UpdateUserRequest request,
            [FromServices] IUserService userService,
            [FromServices] IValidator<UpdateUserRequest> validator,
            [FromServices] ICurrentUserService currentUserService)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var response = await userService.UpdateUserAsync(currentUserService.UserId, request);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.BadRequest(response);
        }

        private static async Task<IResult> GetUserByIdAsync(
            [FromRoute] Guid id,
            [FromServices] IUserService userService)
        {
            var response = await userService.GetUserByIdAsync(id);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }

        private static async Task<IResult> GetAllUsersAsync(
            [FromServices] IUserService userService,
            [AsParameters] PaginationRequest paginationRequest)
        {
            var response = await userService.GetAllUsersAsync(paginationRequest);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.BadRequest(response);
        }

        private static async Task<IResult> CreateUserAsync(
            [FromBody] CreateUserRequest request,
            [FromServices] IUserService userService,
            [FromServices] IValidator<CreateUserRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var response = await userService.CreateUserAsync(request);

            return response.IsSuccess
                ? Results.Created($"/api/v1/users/{response.Data?.Id}", response)
                : Results.BadRequest(response);
        }

        private static async Task<IResult> UpdateUserAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateUserRequest request,
            [FromServices] IUserService userService,
            [FromServices] IValidator<UpdateUserRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var response = await userService.UpdateUserAsync(id, request);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }

        private static async Task<IResult> DeleteUserAsync(
            [FromRoute] Guid id,
            [FromServices] IUserService userService)
        {
            var response = await userService.DeleteUserAsync(id);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }

        private static async Task<IResult> UpdateUserStatusAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateUserStatusRequest request,
            [FromServices] IUserService userService,
            [FromServices] IValidator<UpdateUserStatusRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var response = await userService.UpdateUserStatusAsync(id, request);

            return response.IsSuccess
                ? Results.Ok(response)
                : Results.NotFound(response);
        }
    }
}