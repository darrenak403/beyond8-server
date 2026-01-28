using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis
{
    public static class NotificationApis
    {
        public static IEndpointRouteBuilder MapNotificationApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/notifications")
                .MapNotificationRoutes()
                .WithTags("Notifications")
                .RequireRateLimiting("Fixed")
                .RequireAuthorization();

            return builder;
        }

        private static RouteGroupBuilder MapNotificationRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/my-notifications", GetMyNotifications)
                .WithName("GetMyNotifications")
                .WithDescription("Lấy danh sách thông báo của người dùng hiện tại (Admin/Staff/User thông thường)")
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/instructor-notifications", GetInstructorNotifications)
                .WithName("GetInstructorNotifications")
                .WithDescription("Lấy danh sách thông báo cho Instructor (chia 2 phần: User và Instructor)")
                .Produces<ApiResponse<InstructorNotificationResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<InstructorNotificationResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetMyNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationNotificationRequest pagination)
        {
            var userId = currentUserService.UserId;
            var userRoles = GetUserRoles(currentUserService);

            var result = await notificationHistoryService.GetMyNotificationsAsync(
                userId,
                userRoles,
                pagination);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetInstructorNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationNotificationRequest pagination)
        {
            var userId = currentUserService.UserId;

            if (!currentUserService.IsInRole(Role.Instructor))
            {
                return Results.BadRequest(ApiResponse<InstructorNotificationResponse>.FailureResponse(
                    "Chỉ Instructor mới có thể truy cập endpoint này."));
            }

            var result = await notificationHistoryService.GetInstructorNotificationsAsync(userId, pagination);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static List<string> GetUserRoles(ICurrentUserService currentUserService)
        {
            var roles = new List<string>();

            if (currentUserService.IsInRole(Role.Admin))
                roles.Add(Role.Admin);
            if (currentUserService.IsInRole(Role.Staff))
                roles.Add(Role.Staff);
            if (currentUserService.IsInRole(Role.Instructor))
                roles.Add(Role.Instructor);
            if (currentUserService.IsInRole(Role.Student))
                roles.Add(Role.Student);

            return roles;
        }
    }
}
