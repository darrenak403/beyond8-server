using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis;

public static class NotificationApis
{
    public static IEndpointRouteBuilder MapNotificationApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/my-notifications", GetMyNotifications)
            .WithName("GetMyNotifications")
            .WithSummary("Lấy danh sách thông báo của người dùng hiện tại")
            .Produces<ApiResponse<List<NotificationResponse>>>()
            .Produces(400)
            .Produces(401);

        return app;
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
