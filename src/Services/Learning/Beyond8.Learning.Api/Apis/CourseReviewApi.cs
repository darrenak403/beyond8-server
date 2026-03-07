using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.CourseReview;
using Beyond8.Learning.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Learning.Api.Apis;

public static class CourseReviewApi
{
    public static IEndpointRouteBuilder MapCourseReviewApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/course-reviews")
            .MapCourseReviewRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Course Review Api");

        return builder;
    }

    public static RouteGroupBuilder MapCourseReviewRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCourseReviewsAsync)
            .WithName("GetCourseReviews")
            .WithDescription("Danh sách đánh giá theo khóa học")
            .AllowAnonymous()
            .Produces<ApiResponse<List<CourseReviewResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseReviewResponse>>>(StatusCodes.Status400BadRequest);

        group.MapPost("/", CreateCourseReview)
            .WithName("CreateCourseReview")
            .WithDescription("Tạo đánh giá khóa học")
            .RequireAuthorization()
            .Produces<ApiResponse<CourseReviewResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseReviewResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetCourseReviewsAsync(
        [FromServices] ICourseReviewService courseReviewService,
        [FromServices] IValidator<GetCourseReviewsRequest> validator,
        [AsParameters] GetCourseReviewsRequest request)
    {
        if (request != null && !request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await courseReviewService.GetReviewsByCourseIdAsync(request!);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateCourseReview(
        [FromBody] CreateCourseReviewRequest request,
        [FromServices] ICourseReviewService courseReviewService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateCourseReviewRequest> validator)
    {
        if (request != null && !request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await courseReviewService.CreateCourseReviewAsync(request!, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}