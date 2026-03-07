# API Design Guide

## RESTful Conventions

### HTTP Methods
| Method | Usage | Idempotent |
|--------|-------|------------|
| GET | Retrieve resource(s) | Yes |
| POST | Create resource | No |
| PUT | Full update | Yes |
| PATCH | Partial update | Yes |
| DELETE | Remove resource | Yes |

### URL Patterns
```
GET    /api/v1/courses              # List (paginated)
GET    /api/v1/courses/{id}         # Get single
POST   /api/v1/courses              # Create
PATCH  /api/v1/courses/{id}         # Update
DELETE /api/v1/courses/{id}         # Delete
GET    /api/v1/courses/{id}/sections # Nested resources
```

### Naming Conventions
- Use plural nouns: `/courses`, `/users`, `/sections`
- Use kebab-case for multi-word: `/course-documents`
- Avoid verbs in URLs (use HTTP methods instead)
- Exception: Actions - `POST /courses/{id}/publish`

## Endpoint Definition

### Minimal API Pattern
```csharp
public static class CourseApis
{
    public static IEndpointRouteBuilder MapCourseApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/courses")
            .MapCourseRoutes()
            .WithTags("Course Api")
            .RequireRateLimiting("Fixed");
        return builder;
    }

    public static RouteGroupBuilder MapCourseRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCoursesAsync)
            .WithName("GetAllCourses")
            .WithDescription("Lấy danh sách khóa học")
            .AllowAnonymous()
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateCourseAsync)
            .WithName("CreateCourse")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }
}
```

## Request/Response DTOs

### Request DTO Pattern
```csharp
// Create request - all required fields
public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
}

// Update request - optional fields
public class UpdateCourseRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
```

### Response DTO Levels
```csharp
// Simple - for lists, search results
public class CourseSimpleResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? AvgRating { get; set; }
}

// Standard - for CRUD operations
public class CourseResponse : CourseSimpleResponse
{
    public string Description { get; set; }
    public List<string> Outcomes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Detail - for full view with related data
public class CourseDetailResponse : CourseResponse
{
    public List<SectionResponse> Sections { get; set; }
    public InstructorResponse Instructor { get; set; }
}
```

## Pagination

### Standard Pagination Request
```csharp
public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsDescending { get; set; }
}

// Extended with filters
public class PaginationCourseSearchRequest : PaginationRequest
{
    public string? Keyword { get; set; }
    public CourseLevel? Level { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### Endpoint Usage
```csharp
private static async Task<IResult> GetAllAsync(
    [FromServices] ICourseService service,
    [AsParameters] PaginationCourseSearchRequest pagination)
{
    var result = await service.GetAllAsync(pagination);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}
```

## Validation with FluentValidation

### Validator Pattern
```csharp
public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Danh mục không được để trống");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá phải lớn hơn hoặc bằng 0");
    }
}
```

### Endpoint Validation
```csharp
private static async Task<IResult> CreateAsync(
    [FromBody] CreateCourseRequest request,
    [FromServices] ICourseService service,
    [FromServices] IValidator<CreateCourseRequest> validator)
{
    if (!request.ValidateRequest(validator, out var validationResult))
        return validationResult!;

    var result = await service.CreateAsync(request);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}
```

## Versioning Strategy

### URL Versioning
```
/api/v1/courses    # Current stable
/api/v2/courses    # Breaking changes
```

### When to Version
- Breaking changes in request/response schema
- Removing required fields
- Changing authentication method
- Major behavior changes

### Backward Compatibility
- Add new optional fields (non-breaking)
- Add new endpoints (non-breaking)
- Deprecate before removing
