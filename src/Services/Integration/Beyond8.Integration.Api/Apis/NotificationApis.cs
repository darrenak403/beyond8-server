using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
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
            group.MapGet("/student", GetStudentNotifications)
                .WithName("GetStudentNotifications")
                .WithDescription("Lấy danh sách thông báo cho Student Dashboard")
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/instructor", GetInstructorContextNotifications)
                .WithName("GetInstructorContextNotifications")
                .WithDescription("Lấy danh sách thông báo cho Instructor Dashboard")
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            // Staff endpoints
            group.MapGet("/staff", GetStaffNotifications)
                .WithName("GetStaffNotifications")
                .WithDescription("Lấy danh sách thông báo cho Staff Dashboard")
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<NotificationResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(x => x.RequireRole(Role.Staff, Role.Admin));

            group.MapGet("/student/status", GetStudentNotificationStatus)
                .WithName("GetStudentNotificationStatus")
                .WithDescription("Lấy trạng thái thông báo cho Student Dashboard")
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/instructor/status", GetInstructorNotificationStatus)
                .WithName("GetInstructorNotificationStatus")
                .WithDescription("Lấy trạng thái thông báo cho Instructor Dashboard")
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/staff/status", GetStaffNotificationStatus)
                .WithName("GetStaffNotificationStatus")
                .WithDescription("Lấy trạng thái thông báo cho Staff Dashboard")
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<NotificationStatusResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(x => x.RequireRole(Role.Staff, Role.Admin));

            group.MapPut("/student/read-all", ReadAllStudentNotifications)
                .WithName("ReadAllStudentNotifications")
                .WithDescription("Đánh dấu tất cả thông báo Student đã đọc")
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/instructor/read-all", ReadAllInstructorNotifications)
                .WithName("ReadAllInstructorNotifications")
                .WithDescription("Đánh dấu tất cả thông báo Instructor đã đọc")
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}/read", ReadNotification)
                .WithName("ReadNotification")
                .WithDescription("Đánh dấu thông báo đã đọc")
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("{id:guid}/unread", UnreadNotification)
                .WithName("UnreadNotification")
                .WithDescription("Đánh dấu thông báo chưa đọc")
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteNotification)
                .WithName("DeleteNotification")
                .WithDescription("Xóa thông báo")
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            // Admin endpoints
            group.MapGet("/admin/logs", GetAllNotificationLogs)
                .WithName("GetAllNotificationLogs")
                .WithDescription("Lấy danh sách log thông báo toàn hệ thống (không hiển thị nội dung chi tiết)")
                .Produces<ApiResponse<List<NotificationLogResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<NotificationLogResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization(x => x.RequireRole(Role.Admin));

            return group;
        }

        private static async Task<IResult> GetAllNotificationLogs(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [AsParameters] PaginationNotificationRequest pagination)
        {
            var result = await notificationHistoryService.GetAllNotificationLogsAsync(pagination);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetStaffNotificationStatus(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.GetNotificationStatusAsync(
                currentUserService.UserId,
                NotificationContext.Staff);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetStaffNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationNotificationRequest pagination
        )
        {
            var result = await notificationHistoryService.GetNotificationsByContextAsync(
                currentUserService.UserId,
                NotificationContext.Staff,
                pagination);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetStudentNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationNotificationRequest pagination)
        {
            var result = await notificationHistoryService.GetNotificationsByContextAsync(
                currentUserService.UserId,
                NotificationContext.Student,
                pagination);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetInstructorContextNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationNotificationRequest pagination)
        {
            if (!currentUserService.IsInRole(Role.Instructor))
            {
                return Results.BadRequest(ApiResponse<List<NotificationResponse>>.FailureResponse(
                    "Chỉ Instructor mới có thể truy cập endpoint này."));
            }

            var result = await notificationHistoryService.GetNotificationsByContextAsync(
                currentUserService.UserId,
                NotificationContext.Instructor,
                pagination);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetStudentNotificationStatus(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.GetNotificationStatusAsync(
                currentUserService.UserId,
                NotificationContext.Student);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetInstructorNotificationStatus(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            if (!currentUserService.IsInRole(Role.Instructor))
            {
                return Results.BadRequest(ApiResponse<NotificationStatusResponse>.FailureResponse(
                    "Chỉ Instructor mới có thể truy cập endpoint này."));
            }

            var result = await notificationHistoryService.GetNotificationStatusAsync(
                currentUserService.UserId,
                NotificationContext.Instructor);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ReadAllStudentNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.ReadAllNotificationAsync(
                currentUserService.UserId,
                NotificationContext.Student);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ReadAllInstructorNotifications(
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            if (!currentUserService.IsInRole(Role.Instructor))
            {
                return Results.BadRequest(ApiResponse<bool>.FailureResponse(
                    "Chỉ Instructor mới có thể truy cập endpoint này."));
            }

            var result = await notificationHistoryService.ReadAllNotificationAsync(
                currentUserService.UserId,
                NotificationContext.Instructor);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteNotification(
            [FromRoute] Guid id,
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.DeleteNotificationAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UnreadNotification(
            [FromRoute] Guid id,
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.UnreadNotificationAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ReadNotification(
            [FromRoute] Guid id,
            [FromServices] INotificationHistoryService notificationHistoryService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await notificationHistoryService.ReadNotificationAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

    }
}
