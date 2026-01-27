using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

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
            .AllowAnonymous()
            .Produces<ApiResponse<List<CategoryTreeDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CategoryTreeDto>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetAllCategoriesAsync)
            .WithName("GetAllCategories")
            .WithDescription("Lấy danh sách danh mục")
            .AllowAnonymous()
            .Produces<ApiResponse<List<CategoryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CategoryResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}", GetCategoryByIdAsync)
            .WithName("GetCategoryById")
            .WithDescription("Lấy thông tin danh mục theo ID")
            .AllowAnonymous()
            .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/parent/{parentId}", GetCategoriesByParentIdAsync)
            .WithName("GetCategoriesByParentId")
            .WithDescription("Lấy danh sách danh mục con theo ID danh mục cha")
            .AllowAnonymous()
            .Produces<ApiResponse<List<CategoryResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CategoryResponse>>>(StatusCodes.Status400BadRequest);

        group.MapPost("/", CreateCategoryAsync)
            .WithName("CreateCategory")
            .WithDescription("Tạo danh mục mới")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<CategorySimpleResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CategorySimpleResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id}", UpdateCategoryAsync)
            .WithName("UpdateCategory")
            .WithDescription("Cập nhật thông tin danh mục")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CategoryResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id}", DeleteCategoryAsync)
            .WithName("DeleteCategory")
            .WithDescription("Xóa danh mục")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id}/toggle-status", ToggleCategoryStatusAsync)
            .WithName("ToggleCategoryStatus")
            .WithDescription("Kích hoạt/vô hiệu hóa danh mục")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetCategoryTreeAsync(
        [FromServices] ICategoryService categoryService)
    {
        var result = await categoryService.GetCategoryTreeAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetAllCategoriesAsync(
        [FromServices] ICategoryService categoryService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await categoryService.GetAllCategoriesAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCategoryByIdAsync(
        Guid id,
        [FromServices] ICategoryService categoryService)
    {
        var result = await categoryService.GetCategoryByIdAsync(id);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCategoriesByParentIdAsync(
        Guid parentId,
        [FromServices] ICategoryService categoryService)
    {
        var result = await categoryService.GetCategoriesByParentIdAsync(parentId);
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

    private static async Task<IResult> UpdateCategoryAsync(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        [FromServices] ICategoryService categoryService,
        [FromServices] IValidator<UpdateCategoryRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await categoryService.UpdateCategoryAsync(id, request);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteCategoryAsync(
        Guid id,
        [FromServices] ICategoryService categoryService)
    {
        var result = await categoryService.DeleteCategoryAsync(id);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> ToggleCategoryStatusAsync(
        Guid id,
        [FromServices] ICategoryService categoryService)
    {
        var result = await categoryService.ToggleCategoryStatusAsync(id);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
}
