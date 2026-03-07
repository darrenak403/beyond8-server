# Code Quality Standards

## Error Handling

### ApiResponse Pattern
```csharp
// Service methods return ApiResponse<T>, never throw for business errors
public async Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request)
{
    try
    {
        // Validation
        var category = await unitOfWork.CategoryRepository.FindByIdAsync(request.CategoryId);
        if (category == null)
            return ApiResponse<CourseResponse>.FailureResponse("Danh mục không tồn tại");

        // Business logic
        var course = request.ToEntity();
        await unitOfWork.CourseRepository.AddAsync(course);
        await unitOfWork.SaveChangesAsync();

        return ApiResponse<CourseResponse>.SuccessResponse(
            course.ToResponse(),
            "Tạo khóa học thành công");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating course");
        return ApiResponse<CourseResponse>.FailureResponse("Đã xảy ra lỗi khi tạo khóa học");
    }
}
```

### Global Exception Middleware
Handles unexpected exceptions at API level:
- UnauthorizedAccessException → 401
- ArgumentException → 400
- KeyNotFoundException → 404
- Other → 500

## Validation

### FluentValidation Rules
```csharp
public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        // Required fields
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        // Format validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ");

        // Range validation
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá phải >= 0")
            .LessThanOrEqualTo(100000000).WithMessage("Giá không được vượt quá 100 triệu");

        // Pattern validation
        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Mật khẩu tối thiểu 8 ký tự")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Mật khẩu phải có chữ thường, chữ hoa và số");

        // Conditional validation
        When(x => x.IsPremium, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Khóa học premium phải có giá > 0");
        });
    }
}
```

### Custom Validation Methods
```csharp
// Extract reusable validation logic
private async Task<(bool IsValid, string? ErrorMessage)> ValidateCategoryExistsAsync(Guid categoryId)
{
    var category = await unitOfWork.CategoryRepository.FindByIdAsync(categoryId);
    if (category == null)
        return (false, "Danh mục không tồn tại");
    if (!category.IsActive)
        return (false, "Danh mục đã bị vô hiệu hóa");
    return (true, null);
}

// Usage
var validation = await ValidateCategoryExistsAsync(request.CategoryId);
if (!validation.IsValid)
    return ApiResponse<T>.FailureResponse(validation.ErrorMessage!);
```

## Logging

### Structured Logging
```csharp
// Good - structured parameters
logger.LogInformation("User {UserId} created course {CourseId}", userId, courseId);
logger.LogError(ex, "Failed to create course for user {UserId}", userId);

// Bad - string interpolation
logger.LogInformation($"User {userId} created course {courseId}");
```

### Log Levels
| Level | Use Case |
|-------|----------|
| Debug | Detailed debugging info |
| Information | Normal operations |
| Warning | Unusual but handled situations |
| Error | Errors that need attention |
| Critical | System failures |

## Async/Await

### Rules
```csharp
// Always use async for I/O
public async Task<ApiResponse<T>> GetAsync() { ... }

// Never use .Result or .Wait()
var result = await service.GetAsync();  // Good
var result = service.GetAsync().Result; // Bad - deadlock risk

// Suffix async methods
public async Task<T> GetByIdAsync(Guid id)
public async Task SaveChangesAsync()
```

## Naming Conventions

### C# Conventions
```csharp
// PascalCase: Classes, Methods, Properties
public class CourseService
public async Task<T> GetByIdAsync(Guid id)
public string Title { get; set; }

// camelCase: Parameters, local variables
public void Process(Guid courseId) { var localVar = 1; }

// Interfaces: I prefix
public interface ICourseService
public interface IUnitOfWork

// Async suffix
public async Task SaveChangesAsync()
```

### File Organization
```
Application/
├── Dtos/
│   ├── Courses/
│   │   ├── CourseResponse.cs
│   │   ├── CreateCourseRequest.cs
│   │   └── PaginationCourseSearchRequest.cs
│   └── Sections/
├── Services/
│   ├── Interfaces/
│   │   └── ICourseService.cs
│   └── Implements/
│       └── CourseService.cs
├── Mappings/
│   └── CourseMappings/
│       └── CourseMappings.cs
└── Validators/
    └── Courses/
        └── CreateCourseRequestValidator.cs
```

## DRY Principle

### Extract Common Logic
```csharp
// Bad - duplicate validation
public async Task Method1()
{
    var user = await repo.FindByIdAsync(userId);
    if (user == null) return Failure("User not found");
    if (!user.IsActive) return Failure("User inactive");
}
public async Task Method2()
{
    var user = await repo.FindByIdAsync(userId);
    if (user == null) return Failure("User not found");
    if (!user.IsActive) return Failure("User inactive");
}

// Good - extracted helper
private async Task<(bool IsValid, User? User, string? Error)> ValidateUserAsync(Guid userId)
{
    var user = await repo.FindByIdAsync(userId);
    if (user == null) return (false, null, "User not found");
    if (!user.IsActive) return (false, null, "User inactive");
    return (true, user, null);
}
```

## Security

### Authorization
```csharp
// Endpoint level
.RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))

// Service level
if (!currentUserService.IsInRole(Role.Admin))
    return ApiResponse<T>.FailureResponse("Không có quyền truy cập");

// Resource ownership
if (course.InstructorId != currentUserId)
    return ApiResponse<T>.FailureResponse("Không có quyền chỉnh sửa khóa học này");
```

### Password Handling
```csharp
// Always hash passwords
var hasher = new PasswordHasher<User>();
user.PasswordHash = hasher.HashPassword(user, password);

// Verify password
var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
```

### Sensitive Data
- Never log passwords, tokens, or PII
- Use User Secrets for local development
- Use environment variables in production
