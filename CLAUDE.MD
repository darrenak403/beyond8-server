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
│   │   └── Catalog/                      # Course catalog management
│   │       ├── Beyond8.Catalog.Api/
│   │       ├── Beyond8.Catalog.Application/
│   │       ├── Beyond8.Catalog.Domain/
│   │       └── Beyond8.Catalog.Infrastructure/
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

## Services

### Identity Service

Handles authentication, user management, instructor profiles, and subscriptions:

#### Authentication Features

- User registration with OTP verification
- Login with JWT token generation
- Password management (reset, change, forgot password)
- Refresh token implementation

#### User Management Features

- User profile management (CRUD operations)
- User status management (Active, Inactive, Banned)
- Avatar and cover image management
- Admin user management

#### Instructor Management Features

- Instructor profile creation and verification workflow
- Profile submission for approval
- Admin approval/rejection of instructor applications
- Instructor verification status tracking (Pending, Approved, Rejected, Hidden)

#### Subscription Features

- Subscription plan management
- User subscription tracking
- AI usage quota management

#### API Endpoints

**Auth Endpoints** (`/api/v1/auth`):

- `POST /register` - Register new user
- `POST /verify-otp` - Verify OTP for registration
- `POST /resend-otp` - Resend OTP
- `POST /login` - User login
- `POST /refresh-token` - Refresh JWT token
- `POST /forgot-password` - Request password reset
- `POST /verify-forgot-password-otp` - Verify forgot password OTP
- `POST /reset-password` - Reset password
- `POST /change-password` - Change password (authenticated)

**User Endpoints** (`/api/v1/users`):

- `GET /me` - Get current user profile
- `PATCH /me` - Update current user profile
- `GET /` - Get all users (Admin only, paginated)
- `GET /{id}` - Get user by ID
- `POST /` - Create user (Admin only)
- `PATCH /{id}` - Update user (Admin only)

**Instructor Endpoints** (`/api/v1/instructors`):

- `GET /check` - Check instructor application status
- `POST /apply` - Apply to become instructor
- `GET /profile` - Get own instructor profile
- `PATCH /profile` - Update instructor profile
- `POST /profile/submit` - Submit profile for approval
- `GET /admin/pending` - Get pending applications (Admin/Staff)
- `GET /admin/all` - Get all instructor profiles (Admin/Staff)
- `POST /admin/{id}/approve` - Approve instructor (Admin/Staff)
- `POST /admin/{id}/reject` - Reject instructor (Admin/Staff)
- `POST /admin/{id}/hide` - Hide instructor (Admin/Staff)

**Subscription Endpoints** (`/api/v1/subscriptions`):

- `GET /my-subscription` - Get current user's subscription
- `GET /plans` - Get all subscription plans

### Integration Service

Handles media file uploads, storage management using AWS S3, AI integration, notifications, email, and eKYC:

#### Media File Management Features

- **Presigned URL Generation**: Generate secure upload URLs for client-side file uploads
- **File Upload Workflow**:
  1. Client requests presigned URL with file metadata
  2. Client uploads file directly to S3 using presigned URL
  3. Client confirms upload completion
  4. System verifies file exists in S3 and updates database
- **File Management**: Get, list, and delete user files
- **Folder Organization**: Organize files by type (avatars, certificates, identity cards)
- **File Status Tracking**: Track upload status (Pending, Uploaded, Failed, Deleted)

#### AI Integration Features

- **AI Usage Tracking**: Track AI API calls (tokens, costs, providers)
- **AI Prompt Management**: Store and manage reusable AI prompts
- **Gemini Integration**: Integration with Google's Gemini AI service
- **Quiz Generation**: AI-powered quiz generation from course content
- **Document Embedding**: Vector embeddings for course documents (RAG support)
- **Profile Review**: AI-assisted instructor profile review
- **Usage Analytics**: Statistics and reports on AI usage per user or system-wide

#### Notification Features

- **Push Notifications**: Firebase Cloud Messaging (FCM) integration
- **Notification History**: Track and retrieve user notifications
- **Read/Unread Status**: Mark notifications as read

#### Email Features

- **Transactional Emails**: Send OTP, approval, and notification emails
- **Email Templates**: Pre-defined templates for different email types
- **Event-Driven**: Triggered via MassTransit consumers

#### VNPT eKYC Features

- **ID Card OCR**: Extract information from Vietnamese ID cards
- **ID Classification**: Classify ID card types
- **Liveness Detection**: Verify user is a real person

#### API Endpoints

**Media File Endpoints** (`/api/v1/media`):
All endpoints require authentication.

**Upload Endpoints** (POST with presigned URL generation):

- `/avatar/presigned-url` - Upload user avatar (max 5MB, images only)
- `/certificate/presigned-url` - Upload instructor certificates (max 10MB, PDF/images)
- `/identity-card/front/presigned-url` - Upload front side of ID card (max 10MB, PDF/images)
- `/identity-card/back/presigned-url` - Upload back side of ID card (max 10MB, PDF/images)

**File Management**:

- `POST /confirm` - Confirm file upload completion
- `GET /{fileId}` - Get file information by ID
- `GET /folder/{folder}` - Get all user files in a specific folder
- `DELETE /{fileId}` - Delete a file (soft delete)

**AI Usage Endpoints** (`/api/v1/ai-usage`):
All endpoints require authentication. Admin-only endpoints require "Admin" role.

- `GET /my-usage` - Get current user's AI usage history (paginated)
- `GET /all` - Get all AI usage records (Admin only, paginated)
- `GET /user/{userId}` - Get specific user's AI usage history (Admin only, paginated)
- `GET /statistics` - Get overall AI usage statistics (Admin only)
- `GET /by-date-range?startDate={date}&endDate={date}&pageNumber={n}&pageSize={n}` - Get AI usage within date range (Admin only, paginated)

**AI Endpoints** (`/api/v1/ai`):

- `POST /generate-quiz` - Generate quiz from content
- `POST /embed-documents` - Create vector embeddings for documents
- `POST /review-profile` - AI-assisted profile review

**AI Prompt Endpoints** (`/api/v1/ai-prompts`):

- `GET /` - Get all prompts
- `GET /{id}` - Get prompt by ID
- `POST /` - Create new prompt (Admin)
- `PATCH /{id}` - Update prompt (Admin)
- `DELETE /{id}` - Delete prompt (Admin)

**Notification Endpoints** (`/api/v1/notifications`):

- `GET /` - Get user notifications (paginated)
- `GET /status` - Get notification status (unread count)
- `POST /{id}/read` - Mark notification as read
- `POST /read-all` - Mark all notifications as read

**VNPT eKYC Endpoints** (`/api/v1/ekyc`):

- `POST /classify` - Classify ID card type
- `POST /ocr/front` - OCR front side of ID card
- `POST /ocr/back` - OCR back side of ID card
- `POST /liveness` - Liveness detection

#### Storage Configuration

- **Provider**: AWS S3
- **File Key Format**: `{folder}/{userId}/{filename}` or `{folder}/{userId}/{subFolder}/{filename}`
- **Presigned URL Expiration**: 15 minutes
- **File Validation**: Type-specific validators for avatars and documents

#### Validation Rules

**Avatar Upload**:

- Allowed formats: JPEG, JPG, PNG, WebP
- Max size: 5MB

**Document Upload** (Certificates, ID Cards):

- Allowed formats: PDF, JPEG, JPG, PNG
- Max size: 10MB

#### Usage Flow

```csharp
// 1. Request presigned URL
POST /api/v1/media/avatar/presigned-url
{
    "fileName": "avatar.jpg",
    "contentType": "image/jpeg",
    "size": 1024000
}

// Response includes presigned URL and fileId
{
    "isSuccess": true,
    "data": {
        "fileId": "guid",
        "presignedUrl": "https://s3.amazonaws.com/...",
        "expiresAt": "2026-01-19T10:15:00Z"
    }
}

// 2. Upload file to S3 using presigned URL (client-side)
PUT {presignedUrl}
Body: [file binary data]

// 3. Confirm upload
POST /api/v1/media/confirm
{
    "fileId": "guid"
}
```

### Catalog Service

Handles course management, categories, sections, and lessons for the e-learning platform:

#### Category Features

- **Hierarchical Categories**: Support for parent-child category relationships
- **Category Tree**: Get full category tree structure
- **Status Management**: Enable/disable categories

#### Course Features

- **Course Creation**: Instructors can create courses with metadata
- **Course Status Workflow**: Draft → PendingApproval → Approved → Published
- **Course Approval**: Admin/Staff can approve or reject courses
- **Instructor Verification**: Courses require verified instructor status
- **Course Statistics**: Track students, lessons, ratings, and reviews
- **JSONB Fields**: Outcomes, requirements, target audience stored as JSON

#### Section Features

- **Section Management**: CRUD operations for course sections
- **Section Ordering**: Reorder sections within a course
- **Ownership Validation**: Only course owner can modify sections

#### Lesson Features

- **Lesson Management**: CRUD operations for lessons within sections
- **Lesson Types**: Support for different lesson types (Video, Text, Quiz)
- **Lesson Ordering**: Reorder lessons within a section
- **Video Processing**: HLS callback for video lessons
- **Lesson Documents**: Attach documents to lessons

#### API Endpoints

**Category Endpoints** (`/api/v1/categories`):

- `GET /tree` - Get category tree (public)
- `GET /` - Get all categories (public, paginated)
- `GET /{id}` - Get category by ID (public)
- `GET /parent/{parentId}` - Get child categories (public)
- `POST /` - Create category (Admin/Staff)
- `PUT /{id}` - Update category (Admin/Staff)
- `DELETE /{id}` - Delete category (Admin/Staff)
- `PATCH /{id}/toggle-status` - Toggle category status (Admin/Staff)

**Course Endpoints** (`/api/v1/courses`):

- `GET /` - Get all published courses (public, paginated with search)
- `GET /{id}` - Get course by ID (Instructor)
- `POST /` - Create course (Instructor, verified only)
- `DELETE /{id}` - Delete course (Instructor, owner only)
- `GET /instructor` - Get instructor's courses (Instructor)
- `GET /instructor/stats` - Get instructor's course statistics
- `POST /{id}/submit-approval` - Submit for approval (Instructor)
- `PATCH /{id}/metadata` - Update course metadata (Instructor)
- `GET /admin` - Get all courses for admin (Admin/Staff)
- `POST /{id}/approve` - Approve course (Admin/Staff)
- `POST /{id}/reject` - Reject course (Admin/Staff)
- `POST /{id}/publish` - Publish approved course (Instructor)
- `POST /{id}/unpublish` - Unpublish course (Instructor)

**Section Endpoints** (`/api/v1/sections`):

- `GET /course/{courseId}` - Get sections by course (Instructor)
- `GET /{id}` - Get section by ID (Instructor)
- `POST /` - Create section (Instructor)
- `PATCH /{id}` - Update section (Instructor)
- `DELETE /{id}` - Delete section (Instructor)
- `POST /reorder` - Reorder sections (Instructor)

**Lesson Endpoints** (`/api/v1/lessons`):

- `GET /section/{sectionId}` - Get lessons by section (Instructor)
- `GET /{id}` - Get lesson by ID (Instructor)
- `POST /` - Create lesson (Instructor)
- `PATCH /{id}` - Update lesson (Instructor)
- `DELETE /{id}` - Delete lesson (Instructor)
- `POST /reorder` - Reorder lessons (Instructor)
- `POST /video/callback` - HLS video callback (internal)

#### Course Status Workflow

```
Draft → PendingApproval → Approved → Published
                       ↘ Rejected → Draft (can resubmit)
Published → Unpublished (Hidden) → Published
```

#### Domain Entities

- **Category**: Hierarchical course categories
- **Course**: Main course entity with metadata and statistics
- **Section**: Course sections (chapters)
- **Lesson**: Individual lessons within sections
- **CourseDocument**: Attached documents for courses
- **LessonDocument**: Attached documents for lessons

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

- `OtpEmailEvent` - Trigger OTP email sending
- `InstructorProfileSubmittedEvent` - Instructor submits profile for review
- `InstructorApprovalEvent` - Instructor profile approved
- `InstructorHiddenEvent` - Instructor profile hidden
- `InstructorUpdateRequestEvent` - Instructor requests profile update

**Consumers:**

- Identity Service publishes events
- Integration Service consumes events (emails, notifications)
- Catalog Service consumes instructor status events

**Retry Policy:**

- Exponential backoff: 5 retries
- Min interval: 2 seconds
- Max interval: 30 seconds

### Inter-Service Communication

Services communicate via:

1. **HTTP Clients**: For synchronous requests (e.g., `IIdentityClient`)
2. **MassTransit Events**: For asynchronous operations (emails, notifications)

**HTTP Client Pattern:**

```csharp
public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid userId);
    Task<ApiResponse<SubscriptionResponse>> GetUserSubscriptionAsync(Guid userId);
}
```

## Database

- **Type**: PostgreSQL
- **Connection Strings**: Stored in appsettings.json
- **Migrations**: Applied on startup in Development environment
- **Context Factory**: Used for creating contexts in migrations
- **Each Service**: Has its own database (database-per-service pattern)

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
- **Current Branch**: `agitated-pike`
- **Worktree Path**: `C:\Users\hoade\.claude-worktrees\beyond8-server\agitated-pike`
- **Main Repository**: `D:\Spring-2026\SWD392\Beyond8\beyond8-server`

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
