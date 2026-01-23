using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Services.Interfaces;
using FluentValidation;
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
            group.MapPost("/apply", SubmitInstructorProfileAsync)
                .WithName("ApplyAsInstructor")
                .WithDescription("Gửi đơn đăng ký trở thành giảng viên")
                .RequireAuthorization()
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{id:Guid}/approve", ApproveInstructorProfileAsync)
                .WithName("ApproveApplication")
                .WithDescription("Duyệt/Phê duyệt đơn đăng ký giảng viên (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/check-apply", CheckApplyInstructorProfileAsync)
                .WithName("CheckApplyInstructorProfile")
                .WithDescription("Kiểm tra xem người dùng đã gửi đơn đăng ký trở thành giảng viên chưa")
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{id:Guid}/not-approve", NotApproveInstructorProfileAsync)
                .WithName("NotApproveInstructorProfile")
                .WithDescription("Từ chối hoặc yêu cầu cập nhật hồ sơ giảng viên (Admin only, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/admin", GetInstructorProfilesForAdminAsync)
                .WithName("GetInstructorProfilesByStatusForAdmin")
                .WithDescription("Lấy danh sách hồ sơ theo trạng thái xác minh (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<List<InstructorProfileAdminResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<InstructorProfileAdminResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/{id:Guid}/admin", GetInstructorProfileByIdForAdminAsync)
                .WithName("GetInstructorProfileByIdForAdmin")
                .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên theo ID dành cho Admin (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<InstructorProfileAdminResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileAdminResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/me", GetMyInstructorProfileAsync)
                .WithName("GetMyInstructorProfile")
                .WithDescription("Lấy hồ sơ giảng viên của riêng tôi (Require Authorization)")
                .RequireAuthorization()
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/me", UpdateInstructorProfileAsync)
                .WithName("UpdateMyInstructorProfile")
                .WithDescription("Cập nhật hồ sơ giảng viên của riêng tôi (Require Authorization)")
                .RequireAuthorization()
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:Guid}", GetInstructorProfileByIdAsync)
                .WithName("GetInstructorProfileById")
                .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên theo ID (Public)")
                .AllowAnonymous()
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:Guid}", DeleteInstructorProfileAsync)
                .WithName("DeleteInstructorProfile")
                .WithDescription("Xóa/Ẩn hồ sơ giảng viên (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden);

            return group;
        }

        private static async Task<IResult> DeleteInstructorProfileAsync(
            [FromRoute] Guid id,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.DeleteInstructorProfileAsync(id, currentUserService.UserId);
            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }

        private static async Task<IResult> CheckApplyInstructorProfileAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.CheckApplyInstructorProfileAsync(currentUserService.UserId);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.BadRequest(response);
        }

        private static async Task<IResult> GetInstructorProfileByIdForAdminAsync(
            [FromRoute] Guid id,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetInstructorProfileByIdForAdminAsync(id);
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
            [FromRoute] Guid id,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetInstructorProfileByIdAsync(id);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.NotFound(response);
        }

        private static async Task<IResult> GetInstructorProfilesForAdminAsync(
            [FromServices] IInstructorService instructorService,
            [AsParameters] PaginationInstructorRequest paginationRequest)
        {
            var response = await instructorService.GetInstructorProfilesForAdminAsync(paginationRequest);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.BadRequest(response);
        }

        private static async Task<IResult> SubmitInstructorProfileAsync(
            [FromBody] CreateInstructorProfileRequest request,
            [FromServices] IInstructorService instructorService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<CreateInstructorProfileRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.SubmitInstructorProfileAsync(request, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }

        private static async Task<IResult> ApproveInstructorProfileAsync(
            [FromRoute] Guid id,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.ApproveInstructorProfileAsync(id, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }

        private static async Task<IResult> NotApproveInstructorProfileAsync(
            [FromRoute] Guid id,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IInstructorService instructorService,
            [FromBody] NotApproveInstructorProfileRequest request,
            [FromServices] IValidator<NotApproveInstructorProfileRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.NotApproveInstructorProfileAsync(id, request, currentUserService.UserId);

            return response.IsSuccess
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
        }
    }
}
