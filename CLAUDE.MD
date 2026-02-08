# Beyond8 Server - Claude AI Context

## Project Overview

Beyond8 is a microservices-based ASP.NET Core application following Clean Architecture principles. The system is built using .NET Aspire for orchestration and consists of multiple services handling different business domains.

## Technology Stack

- **Framework**: ASP.NET Core (with .NET Aspire)
- **Architecture**: Clean Architecture with Microservices
- **Database**: PostgreSQL
- **Caching**: Redis (via ICacheService)
- **Messaging**: RabbitMQ with MassTransit
- **Authentication**: JWT tokens
- **API Style**: Minimal APIs
- **ORM**: Entity Framework Core
- **Notifications**: Firebase Cloud Messaging (FCM)

## Project Structure

```
beyond8-server/
├── src/
│   ├── Orchestration/
│   │   ├── Beyond8.AppHost/              # .NET Aspire orchestration host
│   │   └── Beyond8.ServiceDefaults/      # Shared service defaults
│   ├── Services/
│   │   ├── Identity/                     # Authentication & user management
│   │   │   ├── Beyond8.Identity.Api/
│   │   │   ├── Beyond8.Identity.Application/
│   │   │   ├── Beyond8.Identity.Domain/
│   │   │   └── Beyond8.Identity.Infrastructure/
│   │   ├── Integration/                  # Integration service (Media, AI, Notifications)
│   │   │   ├── Beyond8.Integration.Api/
│   │   │   ├── Beyond8.Integration.Application/
│   │   │   ├── Beyond8.Integration.Domain/
│   │   │   └── Beyond8.Integration.Infrastructure/
│   │   ├── Catalog/                      # Course catalog management
│   │   │   ├── Beyond8.Catalog.Api/
│   │   │   ├── Beyond8.Catalog.Application/
│   │   │   ├── Beyond8.Catalog.Domain/
│   │   │   └── Beyond8.Catalog.Infrastructure/
│   │   ├── Assessment/                   # Quiz & assignment management
│   │   │   ├── Beyond8.Assessment.Api/
│   │   │   ├── Beyond8.Assessment.Application/
│   │   │   ├── Beyond8.Assessment.Domain/
│   │   │   └── Beyond8.Assessment.Infrastructure/
│   │   ├── Learning/                     # Enrollment, progress & certificates
│   │   │   ├── Beyond8.Learning.Api/
│   │   │   ├── Beyond8.Learning.Application/
│   │   │   ├── Beyond8.Learning.Domain/
│   │   │   └── Beyond8.Learning.Infrastructure/
│   │   ├── Sale/                         # Orders, payments, coupons & wallets
│   │   │   ├── Beyond8.Sale.Api/
│   │   │   ├── Beyond8.Sale.Application/
│   │   │   ├── Beyond8.Sale.Domain/
│   │   │   └── Beyond8.Sale.Infrastructure/
│   │   └── Analytic/                     # Analytics & reporting (stub)
│   │       ├── Beyond8.Analytic.Api/
│   │       ├── Beyond8.Analytic.Application/
│   │       ├── Beyond8.Analytic.Domain/
│   │       └── Beyond8.Analytic.Infrastructure/
├── shared/
│   ├── Beyond8.Common/                   # Common utilities and shared code
│   └── Beyond8.DatabaseMigrationHelpers/ # Database migration helpers
└── beyond8-server.sln
```

## Clean Architecture Layers

Each service follows Clean Architecture with four distinct layers:

### 1. Domain Layer (`*.Domain`)

- **Purpose**: Core business logic and entities
- **Contains**:
  - Domain entities (inherit from `BaseEntity`)
  - Repository interfaces
  - Domain enums
  - Business rules
- **Dependencies**: None (completely independent)

### 2. Application Layer (`*.Application`)

- **Purpose**: Business logic and use cases
- **Contains**:
  - DTOs (Data Transfer Objects)
  - Service interfaces and implementations
  - Mapping extensions
  - Validation logic
- **Dependencies**: Domain layer only

### 3. Infrastructure Layer (`*.Infrastructure`)

- **Purpose**: External concerns and data persistence
- **Contains**:
  - DbContext implementations
  - Repository implementations
  - External service integrations
  - Migration configurations
- **Dependencies**: Domain and Application layers

### 4. API Layer (`*.Api`)

- **Purpose**: HTTP endpoints and API configuration
- **Contains**:
  - Minimal API endpoints
  - Middleware configuration
  - OpenAPI/Swagger setup
  - Program.cs configuration
- **Dependencies**: Application and Infrastructure layers

## Key Architectural Patterns

### 1. Repository Pattern with Unit of Work

- All data access goes through `IUnitOfWork`
- Generic repository (`IGenericRepository<T>`) for common CRUD operations
- Specific repositories expose domain-specific queries
- Example: `_unitOfWork.UserRepository.FindOneAsync(u => u.Email == email)`

### 2. ApiResponse Wrapper

All API responses use a consistent wrapper pattern:

```csharp
// Success response
ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully")

// Failure response
ApiResponse<UserDto>.FailureResponse("User not found")

// Paginated response
ApiResponse<List<UserDto>>.SuccessPagedResponse(users, totalCount, pageNumber, pageSize, "Users retrieved")
```

### 3. Service Layer Pattern

- Services contain business logic
- Return `ApiResponse<T>` instead of throwing exceptions for business errors
- Handle exceptions and return meaningful error messages
- Use structured logging with `ILogger<T>`

### 4. Dependency Injection

**Service Lifetimes:**

- **Scoped**: Services with database context (IUnitOfWork, application services)
- **Transient**: Stateless services without context dependencies
- **Singleton**: Thread-safe services (caching, configuration)

**Registration:**

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## Important Conventions

### API Endpoints

- Use Minimal APIs with MapGroup for versioning: `/api/v1/...`
- Create static extension methods for endpoint mapping (e.g., `MapAuthApi()`)
- Specify authorization explicitly: `RequireAuthorization()` or `AllowAnonymous()`
- Document with `.Produces<T>()` for OpenAPI generation
- Add descriptive tags and names for documentation

### Pagination

- **ALWAYS use PaginationRequest** for endpoints that return lists of data
- Use `[AsParameters]` attribute to bind pagination parameters from query string
- For endpoints requiring additional filters (e.g., date ranges), create a new DTO that **inherits from PaginationRequest**
- Return paginated responses using `ApiResponse<List<T>>.SuccessPagedResponse()`

**Example - Standard Pagination:**

```csharp
// Endpoint
private static async Task<IResult> GetUsers(
    [FromServices] IUserService userService,
    [AsParameters] PaginationRequest pagination)
{
    var result = await userService.GetUsersAsync(pagination);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}

// Service
public async Task<ApiResponse<List<UserResponse>>> GetUsersAsync(PaginationRequest pagination)
{
    var users = await _unitOfWork.UserRepository.GetPagedAsync(
        pageNumber: pagination.PageNumber,
        pageSize: pagination.PageSize,
        orderBy: query => query.OrderByDescending(u => u.CreatedAt)
    );

    return ApiResponse<List<UserResponse>>.SuccessPagedResponse(
        users.Items.Select(u => u.ToResponse()).ToList(),
        users.TotalCount,
        pagination.PageNumber,
        pagination.PageSize,
        "Users retrieved successfully");
}
```

**Example - Extended Pagination (with filters):**

```csharp
// DTO inheriting from PaginationRequest
public class DateRangePaginationRequest : PaginationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// Endpoint
private static async Task<IResult> GetByDateRange(
    [FromServices] IService service,
    [AsParameters] DateRangePaginationRequest request)
{
    var result = await service.GetByDateRangeAsync(request);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}

// Service
public async Task<ApiResponse<List<Response>>> GetByDateRangeAsync(DateRangePaginationRequest request)
{
    var items = await _unitOfWork.Repository.GetPagedAsync(
        pageNumber: request.PageNumber,
        pageSize: request.PageSize,
        filter: x => x.CreatedAt >= request.StartDate && x.CreatedAt <= request.EndDate,
        orderBy: query => query.OrderByDescending(x => x.CreatedAt)
    );

    return ApiResponse<List<Response>>.SuccessPagedResponse(
        items.Items.Select(x => x.ToResponse()).ToList(),
        items.TotalCount,
        request.PageNumber,
        request.PageSize,
        "Items retrieved successfully");
}
```

### Error Handling

- Global exception handling via `GlobalExceptionsMiddleware`
- Exception mapping:
  - `UnauthorizedAccessException` → 401 Unauthorized
  - `ArgumentException` → 400 Bad Request
  - `KeyNotFoundException` → 404 Not Found
  - Other exceptions → 500 Internal Server Error

### Async/Await

- Always use async/await for I/O operations
- Never use `.Result` or `.Wait()` (causes deadlocks)
- Return `Task<T>` or `Task`, never async void
- Suffix async methods with `Async` (e.g., `RegisterUserAsync`)

### Logging

Use structured logging with named parameters:

```csharp
// Good
logger.LogInformation("User registered successfully: {Email}", request.Email);

// Bad
logger.LogInformation($"User registered successfully: {request.Email}");
```

### Validation

**FluentValidation (Recommended):**

- Use FluentValidation for all request validation in Minimal APIs
- Create validators in `Application/Validators` folder organized by domain
- Register validators: `services.AddValidatorsFromAssemblyContaining<RegisterRequest>()`
- Inject validators into endpoints: `IValidator<TRequest> validator`
- Validate at endpoint start: `if (!request.ValidateRequest(validator, out var result)) return result!;`
- Provide error messages in Vietnamese for user-facing validation

**Example Validator:**

```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password không được để trống")
            .MinimumLength(8).WithMessage("Password tối thiểu 8 ký tự")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password phải có ít nhất 1 chữ thường, 1 chữ hoa và 1 số");
    }
}
```

### Security

- **Passwords**: Hash with `PasswordHasher<User>`, never store plain text
- **JWT**: Store configuration in appsettings.json, implement refresh tokens
- **Authorization**: Validate on endpoints and in service layer
- **Sensitive Data**: Use User Secrets (dev) or environment variables (prod)

### Role-Based Authorization

The system uses role-based access control with the following roles:

- **Admin**: Full system access
- **Staff**: Limited admin access (approve instructors, manage courses)
- **Instructor**: Create and manage courses
- **Student**: Access and consume courses (default role)

**Authorization Patterns:**

```csharp
// Single role
.RequireAuthorization(x => x.RequireRole(Role.Instructor))

// Multiple roles (OR logic)
.RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))

// Using ICurrentUserService
if (!currentUserService.IsInAnyRole(Role.Admin, Role.Staff))
    return ApiResponse<T>.FailureResponse("Không có quyền truy cập");
```

### Rate Limiting

All API endpoints use rate limiting:

```csharp
.RequireRateLimiting("Fixed")
```

### Caching

- Use `ICacheService` for all caching operations
- Key format: `"prefix:identifier"` (e.g., `"otp_register:{email}"`)
- Set appropriate expiration times
- Handle cache misses gracefully

### Database Operations

- Always use async methods: `FindOneAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`
- Entities inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt)
- Configure relationships in `OnModelCreating`
- Use migrations for schema changes

## Event-Driven Architecture

### MassTransit with RabbitMQ

The system uses MassTransit for message-based communication between services.

**Configuration:**

```csharp
builder.AddMassTransitWithRabbitMq(x =>
{
    x.AddConsumer<OtpEmailConsumer>();
    x.AddConsumer<InstructorApprovalConsumer>();
});
```

**Event Types** (defined in `Beyond8.Common.Events`):

_Identity Events:_

- `OtpEmailEvent` - Trigger OTP email sending
- `InstructorProfileSubmittedEvent` - Instructor submits profile for review
- `InstructorApprovalEvent` - Instructor profile approved
- `InstructorHiddenEvent` - Instructor profile hidden
- `InstructorUpdateRequestEvent` - Instructor requests profile update

_Assessment Events:_

- `AssignmentSubmittedEvent` - Student submits assignment
- `AssignmentGradedEvent` - Assignment manually graded
- `AssignmentDeletedEvent` - Assignment deleted
- `QuizAttemptCompletedEvent` - Student completes quiz attempt
- `AiAssignmentGradedEvent` - AI grading initiated
- `AiGradingCompletedEvent` - AI grading completed

_Learning Events:_

- `CourseEnrollmentCountChangedEvent` - Enrollment count updates
- `CourseCompletedEvent` - Student completes course
- `CourseRatingUpdatedEvent` - Course rating/review updated
- `FreeEnrollmentOrderRequestEvent` - Free course enrollment request

_Sale Events:_

- `OrderCompletedEvent` - Order finalized and paid (triggers enrollment)

_Catalog Events:_

- `LessonQuizUnlinkedEvent` - Quiz unlinked from lesson
- `CacheInvalidateEvent` - Cache invalidation signaling

**Consumers:**

- Identity Service publishes auth/instructor events
- Integration Service consumes events (emails, notifications, AI grading)
- Catalog Service consumes instructor status events, publishes lesson events
- Assessment Service consumes lesson/AI grading events
- Learning Service publishes enrollment/progress events
- Sale Service publishes order events, consumes enrollment requests

**Retry Policy:**

- Exponential backoff: 5 retries
- Min interval: 2 seconds
- Max interval: 30 seconds

### Inter-Service Communication

Services communicate via:

1. **HTTP Clients**: For synchronous requests (with Polly resilience policies)
2. **MassTransit Events**: For asynchronous operations (emails, notifications, enrollment triggers)

**HTTP Client Dependencies:**

| Service    | Calls To | Client Interface  | Purpose                       |
| ---------- | -------- | ----------------- | ----------------------------- |
| Assessment | Learning | `ILearningClient` | Verify student enrollment     |
| Assessment | Catalog  | `ICatalogService` | Get course/lesson context     |
| Learning   | Catalog  | `ICatalogClient`  | Get course information        |
| Learning   | Identity | `IIdentityClient` | Get user information          |
| Sale       | Catalog  | `ICatalogClient`  | Get course pricing/details    |
| Catalog    | Learning | `ILearningClient` | Check enrollment status       |
| Catalog    | Identity | `IIdentityClient` | Check instructor verification |

**HTTP Client Pattern:**

```csharp
public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid userId);
    Task<ApiResponse<SubscriptionResponse>> GetUserSubscriptionAsync(Guid userId);
}
```

All HTTP clients use **Polly resilience policies** with exponential backoff retry and circuit breaker patterns.

## Database

- **Type**: PostgreSQL (database-per-service pattern)
- **Connection Strings**: Stored in appsettings.json
- **Migrations**: Applied on startup in Development environment
- **Context Factory**: Used for creating contexts in migrations

**DbContexts:**

| Service     | DbContext              |
| ----------- | ---------------------- |
| Identity    | `IdentityDbContext`    |
| Integration | `IntegrationDbContext` |
| Catalog     | `CatalogDbContext`     |
| Assessment  | `AssessmentDbContext`  |
| Learning    | `LearningDbContext`    |
| Sale        | `SaleDbContext`        |
| Analytic    | `AnalyticDbContext`    |

## Common Shared Projects

### Beyond8.Common

Contains shared utilities, constants, and helper classes used across all services.

### Beyond8.DatabaseMigrationHelpers

Contains helpers and extensions for database migration management.

## Configuration

- **appsettings.json**: Main configuration file
- **appsettings.Development.json**: Development overrides
- **User Secrets**: For sensitive data in development
- **Environment Variables**: For sensitive data in production

Connection string constants defined in `Const` class (e.g., `Const.IdentityServiceDatabase`).

## Git Workflow

- **Main Branch**: `main`
- **Main Repository**: `D:\Spring-2026\SWD392\Beyond8\beyond8-server`
- **Worktrees**: Feature branches use git worktrees under `C:\Users\hoade\.claude-worktrees\beyond8-server\`

## Development Guidelines

### Before Submitting Code

- [ ] All API endpoints use `ApiResponse<T>` wrapper
- [ ] Services return `ApiResponse<T>` instead of throwing exceptions for business errors
- [ ] All async methods use async/await properly, no `.Result` or `.Wait()`
- [ ] DTOs have validation attributes with meaningful error messages in Vietnamese
- [ ] Logging uses structured logging with parameters, not string interpolation
- [ ] Dependencies are injected through constructors, not service locator
- [ ] Repository pattern and Unit of Work are used for all data access
- [ ] Clean architecture layers are respected (no circular dependencies)
- [ ] Security best practices followed (password hashing, JWT, authorization)
- [ ] Error handling is centralized in middleware
- [ ] Configuration stored in appsettings.json, not hardcoded
- [ ] Code follows consistent naming conventions (PascalCase, camelCase)
- [ ] Database operations use async methods only
- [ ] No duplicate code - extract common validation/logic into reusable private methods

### Naming Conventions

- **PascalCase**: Classes, methods, properties, interfaces
- **camelCase**: Local variables, method parameters
- **Interfaces**: Prefix with `I` (e.g., `IAuthService`, `IUserRepository`)
- **Async Methods**: Suffix with `Async` (e.g., `RegisterUserAsync`)

### File Organization

- Group related files in folders: Dtos, Services, Entities, Repositories, Mappings
- Use descriptive namespaces matching folder structure
- Keep files focused on single responsibility
- Separate interfaces from implementations

### Code Reusability & DRY Principle

**Avoid Duplicate Code:**

- Extract common validation logic into reusable private methods
- Use tuple returns for validation methods: `(bool IsValid, string? ErrorMessage)`
- Create helper methods for repeated operations (e.g., OTP validation, user validation)
- Prefer composition over duplication

**Example - Extract Common Validation:**

```csharp
// Bad - Duplicate validation code in multiple methods
public async Task<ApiResponse<bool>> Method1(Request request)
{
    var cachedOtp = await cacheService.GetAsync<string>($"otp:{request.Email}");
    if (string.IsNullOrEmpty(cachedOtp))
        return ApiResponse<bool>.FailureResponse("OTP không hợp lệ");
    if (cachedOtp != request.OtpCode)
        return ApiResponse<bool>.FailureResponse("OTP không đúng");
    // ... rest of logic
}

// Good - Extract to reusable method
private async Task<(bool IsValid, string? ErrorMessage)> ValidateOtpFromCacheAsync(
    string cacheKey, string otpCode, string email)
{
    var cachedOtp = await cacheService.GetAsync<string>(cacheKey);
    if (string.IsNullOrEmpty(cachedOtp))
        return (false, "OTP không hợp lệ hoặc đã hết hạn");
    if (cachedOtp != otpCode)
        return (false, "OTP không đúng");
    return (true, null);
}

public async Task<ApiResponse<bool>> Method1(Request request)
{
    var validation = await ValidateOtpFromCacheAsync($"otp:{request.Email}", request.OtpCode, request.Email);
    if (!validation.IsValid)
        return ApiResponse<bool>.FailureResponse(validation.ErrorMessage!);
    // ... rest of logic
}
```

**Benefits:**

- Single source of truth for validation logic
- Easier to maintain and update
- Reduces code size and complexity
- Improves testability

## Quick Reference

### Common Patterns

```csharp
// Service Response
ApiResponse<T>.SuccessResponse(data, message)

// Error Response
ApiResponse<T>.FailureResponse(message)

// Paginated Response
ApiResponse<List<T>>.SuccessPagedResponse(items, total, page, size, message)

// Structured Logging
logger.LogInformation("Message with {Parameter}", value)

// Async Query
await repository.FindOneAsync(x => x.Id == id)

// Service Registration
builder.Services.AddScoped<IService, Service>()

// Protected Endpoint
.RequireAuthorization()

// Role-Based Endpoint
.RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))

// Rate Limiting
.RequireRateLimiting("Fixed")

// Validation
[Required(ErrorMessage = "Error message")]

// Get Current User
var currentUserId = currentUserService.UserId;
var email = currentUserService.Email;
var isAdmin = currentUserService.IsInRole(Role.Admin);

// Publish Event
await publishEndpoint.Publish(new OtpEmailEvent(...));

// HTTP Client Call
var result = await identityClient.CheckInstructorProfileVerifiedAsync(userId);
```

## Additional Documentation

For detailed ASP.NET Core best practices and coding standards, refer to:

- `.cursor/skills/asp-rules/SKILL.md` - Comprehensive ASP.NET Core guidelines
- `.github/copilot-instructions.md` - GitHub Copilot instructions
- `docs/TODO-ENROLLMENT-AND-REVIEW.md` - TODO: Kiểm tra enrollment và chế độ review khi student get quiz/lesson

## Notes for AI Assistants

When working with this codebase:

1. Follow Clean Architecture layer boundaries strictly
2. Use the ApiResponse pattern for all service and API responses
3. Apply proper async/await patterns throughout
4. Use structured logging with ILogger
5. Follow the Repository + Unit of Work pattern for data access
6. Respect security best practices (password hashing, JWT validation)
7. Validate inputs at both API and service layers
8. Use dependency injection for all dependencies
9. Keep error handling centralized in middleware
10. Write meaningful error messages in Vietnamese for user-facing validation
11. **Apply DRY principle** - identify and eliminate duplicate code by extracting common logic into reusable private methods
12. Use tuple returns `(bool IsValid, string? ErrorMessage)` for validation helper methods
13. **Use ICurrentUserService** to get authenticated user information in endpoints
14. **Apply rate limiting** to all API endpoints: `.RequireRateLimiting("Fixed")`
15. **Use MassTransit** for cross-service communication (events, not direct HTTP calls when possible)
16. **Check instructor verification** before allowing course-related operations
17. **Generate slugs** for courses using `SlugExtensions` from Beyond8.Common
18. **Store JSON arrays** using JSONB column type for PostgreSQL
