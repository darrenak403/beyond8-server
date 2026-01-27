using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis
{
    public static class CategoryApis
    {
        public static IEndpointRouteBuilder MapCategoryApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/categories")
                .MapCategoryRoutes()
                .RequireRateLimiting("Fixed")
                .WithTags("Category Api");

            return builder;
        }

        public static RouteGroupBuilder MapCategoryRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/tree", GetCategoryTreeAsync)
                .WithName("GetCategoryTree")
                .WithDescription("Lấy cây danh mục")
                .Produces<ApiResponse<List<CategoryTreeDto>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<CategoryTreeDto>>>(StatusCodes.Status400BadRequest);

            group.MapPost("/", CreateCategoryAsync)
                .WithName("CreateCategory")
                .WithDescription("Tạo danh mục mới")
                .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
                .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> GetCategoryTreeAsync(
            [FromServices] ICategoryService categoryService)
        {
            var result = await categoryService.GetCategoryTreeAsync();
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> CreateCategoryAsync(
            [FromBody] CreateCategoryRequest request,
            [FromServices] ICategoryService categoryService,
            [FromServices] IValidator<CreateCategoryRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await categoryService.CreateCategoryAsync(request);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        }
    }
}
