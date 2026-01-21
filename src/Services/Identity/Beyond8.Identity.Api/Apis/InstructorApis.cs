using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Services.Interfaces;
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
            group.MapPost("/registration", SubmitInstructorApplicationAsync)
                 .WithName("SubmitInstructorApplication")
                 .WithDescription("Gửi đơn đăng ký trở thành giảng viên")
                 .RequireAuthorization()
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/{profileId}/approve", ApproveInstructorApplicationAsync)
                 .WithName("ApproveInstructorApplication")
                 .WithDescription("Duyệt đơn đăng ký giảng viên (Admin, Staff only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

            group.MapPost("/{profileId}/not-approve", NotApproveInstructorApplicationAsync)
                 .WithName("NotApproveInstructorApplication")
                 .WithDescription("Từ chối đơn đăng ký giảng viên (Admin only)")
                .RequireAuthorization(x => x.RequireRole(Role.Admin))
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/registration/pending", GetPendingApplicationsAsync)
            .WithName("GetPendingApplications")
            .WithDescription("Lấy danh sách đơn đăng ký giảng viên đang chờ duyệt (Admin, Staff only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            // Note: /me routes must be placed BEFORE /{profileId} to avoid routing conflicts
            group.MapGet("/me", GetMyInstructorProfileAsync)
            .WithName("GetMyInstructorProfile")
            .WithDescription("Lấy hồ sơ giảng viên của tôi (Require Authorization)")
            .RequireAuthorization()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/me", UpdateInstructorProfileAsync)
            .WithName("UpdateInstructorProfile")
            .WithDescription("Cập nhật hồ sơ giảng viên của tôi (Require Authorization)")
            .RequireAuthorization()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("", GetVerifiedInstructorsAsync)
            .WithName("GetVerifiedInstructors")
            .WithDescription("Lấy danh sách giảng viên đã được xác minh (Public - Có phân trang)")
            .AllowAnonymous()
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<InstructorProfileResponse>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/{profileId}", GetInstructorProfileByIdAsync)
            .WithName("GetInstructorProfile")
            .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên (Public)")
            .AllowAnonymous()
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status404NotFound);

            group.MapGet("/{profileId}/admin", GetInstructorProfileByIdForAdminAsync)
            .WithName("GetInstructorProfileForAdmin")
            .WithDescription("Lấy thông tin chi tiết hồ sơ giảng viên cho Admin (Admin, Staff only)")
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

        private static async Task<IResult> GetVerifiedInstructorsAsync(
            [FromServices] IInstructorService instructorService,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return Results.BadRequest(ApiResponse<List<InstructorProfileResponse>>.FailureResponse(
                    "Số trang phải >= 1, kích thước trang phải từ 1-100."));

            var response = await instructorService.GetVerifiedInstructorsAsync(pageNumber, pageSize);
            return response.IsSuccess
                            ? Results.Ok(response)
                            : Results.BadRequest(response);
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

        private static async Task<IResult> GetPendingApplicationsAsync(
            [FromServices] IInstructorService instructorService)
        {
            var response = await instructorService.GetPendingApplicationsAsync();
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
