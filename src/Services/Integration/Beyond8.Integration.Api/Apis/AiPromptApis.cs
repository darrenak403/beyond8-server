using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Prompts;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis
{
    public static class AiPromptApis
    {
        public static IEndpointRouteBuilder MapAiPromptApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/ai-prompts")
                .MapAiPromptRoutes()
                .WithTags("AI Prompt Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapAiPromptRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllPrompts)
                .WithName("GetAllActivePrompts")
                .WithDescription("Lấy tất cả AI prompt templates đang active")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<List<AiPromptResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<AiPromptResponse>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/{id:guid}", GetPromptById)
                .WithName("GetPromptById")
                .WithDescription("Lấy AI prompt template theo ID")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status404NotFound);

            group.MapPost("/", CreatePrompt)
                .WithName("CreatePrompt")
                .WithDescription("Tạo AI prompt template mới (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status201Created)
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdatePrompt)
                .WithName("UpdatePrompt")
                .WithDescription("Cập nhật AI prompt template (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status404NotFound)
                .Produces<ApiResponse<AiPromptResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeletePrompt)
                .WithName("DeletePrompt")
                .WithDescription("Soft delete AI prompt template (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPatch("/{id:guid}/toggle-status", TogglePromptStatus)
                .WithName("TogglePromptStatus")
                .WithDescription("Kích hoạt AI prompt template (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetAllPrompts(
            [FromServices] IAiPromptService promptService,
            [AsParameters] PaginationRequest paginationRequest)
        {
            var result = await promptService.GetAllPromptsAsync(paginationRequest);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetPromptById(
            [FromRoute] Guid id,
            [FromServices] IAiPromptService promptService)
        {
            var result = await promptService.GetPromptByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> CreatePrompt(
            [FromBody] CreateAiPromptRequest request,
            [FromServices] IAiPromptService promptService,
            [FromServices] IValidator<CreateAiPromptRequest> validator,
            [FromServices] ICurrentUserService currentUserService)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await promptService.CreatePromptAsync(request, currentUserService.UserId);

            return result.IsSuccess
                ? Results.Created($"/api/v1/ai-prompts/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdatePrompt(
            [FromRoute] Guid id,
            [FromBody] UpdateAiPromptRequest request,
            [FromServices] IAiPromptService promptService,
            [FromServices] IValidator<UpdateAiPromptRequest> validator,
            [FromServices] ICurrentUserService currentUserService)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await promptService.UpdatePromptAsync(id, request, currentUserService.UserId);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeletePrompt(
            [FromRoute] Guid id,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IAiPromptService promptService)
        {
            var result = await promptService.DeletePromptAsync(id, currentUserService.UserId);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> TogglePromptStatus(
            [FromRoute] Guid id,
            [FromServices] IAiPromptService promptService)
        {
            var result = await promptService.TogglePromptStatusAsync(id);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
