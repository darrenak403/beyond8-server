using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Identity.Api.Apis
{
    public static class InstructorApis
    {
        public static IEndpointRouteBuilder MapInstructorApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/instructors")
                .MapInstructorRoutes()
                .WithTags("Instructor Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapInstructorRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/apply", SubmitInstructorApplicationAsync)
                 .WithName("ApplyAsInstructor")
                 .WithDescription("Gửi đơn đăng ký trở thành giảng viên")
                 .RequireAuthorization()
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{profileId}/approve", ApproveInstructorApplicationAsync)
                 .WithName("ApproveApplication")
                 .WithDescription("Duyệt/Phê duyệt đơn đăng ký giảng viên (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

            group.MapPost("/{profileId}/reject", NotApproveInstructorApplicationAsync)
                 .WithName("RejectApplication")
                 .WithDescription("Từ chối hoặc yêu cầu cập nhật đơn đăng ký giảng viên (Admin only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/by-status", GetInstructorProfilesByStatusAsync)
            .WithName("ListProfilesByStatus")
            .WithDescription("Lấy danh sách hồ sơ theo trạng thái xác minh (Admin, Staff only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/me", GetMyInstructorProfileAsync)
            .WithName("GetMyProfile")
            .WithDescription("Lấy hồ sơ giảng viên của riêng tôi (Require Authorization)")
            .RequireAuthorization()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/me", UpdateInstructorProfileAsync)
            .WithName("UpdateMyProfile")
            .WithDescription("Cập nhật hồ sơ giảng viên của riêng tôi (Require Authorization)")
            .RequireAuthorization()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{profileId}", GetInstructorProfileByIdAsync)
            .WithName("GetProfileById")
            .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên theo ID (Public)")
            .AllowAnonymous()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound);

            group.MapGet("/{profileId}/admin", GetInstructorProfileByIdForAdminAsync)
            .WithName("GetProfileByIdForAdmin")
            .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên theo ID dành cho Admin (Admin, Staff only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<InstructorProfileAdminResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileAdminResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            return group;
        }

        private static async Task<IResult> GetInstructorProfileByIdForAdminAsync(
            [FromRoute] Guid profileId,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetInstructorProfileByIdForAdminAsync(profileId);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.NotFound(response);
        }

        private static async Task<IResult> UpdateInstructorProfileAsync(
            [FromBody] UpdateInstructorProfileRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService,
            [FromServices] IValidator<UpdateInstructorProfileRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.UpdateInstructorProfileAsync(currentUserService.UserId, request);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }

        private static async Task<IResult> GetMyInstructorProfileAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetMyInstructorProfileAsync(currentUserService.UserId);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.NotFound(response);
        }

        private static async Task<IResult> GetInstructorProfileByIdAsync(
            [FromRoute] Guid profileId,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetInstructorProfileByIdAsync(profileId);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.NotFound(response);
        }

        private static async Task<IResult> GetInstructorProfilesByStatusAsync(
            [FromServices] IInstructorService instructorService,
            [FromQuery] VerificationStatus status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return Results.BadRequest(ApiResponse<List<InstructorProfileResponse>>.FailureResponse(
                    "Số trang phải >= 1, kích thước trang phải từ 1-100."));

            var response = await instructorService.GetInstructorProfilesByStatusAsync(status, pageNumber, pageSize);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.BadRequest(response);
        }

        private static async Task<IResult> SubmitInstructorApplicationAsync(
            [FromBody] CreateInstructorProfileRequest request,
            [FromServices] IInstructorService instructorService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<CreateInstructorProfileRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.SubmitInstructorApplicationAsync(request, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);

        }

        private static async Task<IResult> ApproveInstructorApplicationAsync(
        [FromRoute] Guid profileId,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.ApproveInstructorApplicationAsync(profileId, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }

        private static async Task<IResult> NotApproveInstructorApplicationAsync(
        [FromRoute] Guid profileId,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IInstructorService instructorService,
        [FromBody] NotApproveInstructorApplicationRequest request,
        [FromServices] IValidator<NotApproveInstructorApplicationRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.NotApproveInstructorApplicationAsync(profileId, request, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }
    }
}
