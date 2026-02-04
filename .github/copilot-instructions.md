# Beyond8 Server - Copilot AI Context

## Project Overview

Beyond8 is a microservices-based ASP.NET Core application following Clean Architecture principles. The system is built using .NET Aspire for orchestration and consists of multiple services handling different business domains.

## 🛠️ Available MCP Tools

This project is equipped with **Model Context Protocol (MCP)** servers to provide real-time access to system resources. **ALWAYS use these tools FIRST** before making assumptions about the current state of the system.

### 🐘 PostgreSQL MCP

**Connection:** `postgresql://postgres:postgres@localhost:5432/beyond8_identity`

**When to use:**

- Query actual database schema before creating/modifying entities
- Verify table structures, columns, constraints, and indexes
- Check existing data before writing seed scripts
- Validate foreign key relationships
- Inspect migration history in `__EFMigrationsHistory` table

**Example queries:**

```sql
-- Check Users table schema
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'Users';

-- Verify relationships
SELECT tc.constraint_name, tc.table_name, kcu.column_name,
       ccu.table_name AS foreign_table_name, ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY';

-- Check migration status
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 5;
```

### 🐳 Docker MCP

**When to use:**

- Verify service health before debugging connection issues
- Check which containers are running
- Inspect container logs for errors
- Validate port mappings and network configurations
- Monitor resource usage

**Common checks:**

```bash
# List running containers
docker ps

# Check specific service
docker ps --filter "name=beyond8"

# View logs
docker logs <container-id> --tail 50

# Inspect network
docker network inspect bridge
```

### 📁 Filesystem MCP

**Workspace:** `${workspaceFolder}`

**When to use:**

- Navigate project structure efficiently
- Find configuration files (appsettings.json, etc.)
- Locate specific entity, DTO, or service files
- Search for code patterns across multiple files
- Verify file existence before creating new ones

**Efficient search patterns:**

- Find all DbContext: `**/Data/*DbContext.cs`
- Find all entities: `**/Domain/Entities/*.cs`
- Find all DTOs: `**/Application/Dtos/**/*.cs`
- Find config files: `**/appsettings*.json`

### 🔀 Git MCP

**Repository:** `${workspaceFolder}`

**When to use:**

- Review recent commits before making changes
- Check uncommitted changes and staged files
- View file history to understand evolution
- Identify who last modified a file
- Check current branch and status

**Useful commands:**

```bash
# Recent commits
git log --oneline -10

# Current status
git status

# File history
git log --follow <file-path>

# Show changes
git diff HEAD~1
```

### 🌐 Brave Search MCP

**API:** Brave Search API (requires free API key)

**When to use:**

- Find latest documentation for new libraries/frameworks
- Search for recent solutions to specific errors
- Discover best practices from current resources
- Find updated API references (AWS SDK, EF Core, etc.)

**Get API Key:** https://brave.com/search/api/ (2,000 requests/month free)

### 📄 Fetch MCP

**Purpose:** Read web page content after finding it via Brave Search

**Workflow:** Brave Search → Find docs → Fetch → Read content → Implement

### 🧠 Sequential Thinking MCP

**Purpose:** Break complex problems into logical steps

**Use for:** Architecture design, migrations, refactoring, complex debugging

### 💾 Memory MCP

**Purpose:** Remember decisions and patterns across sessions

**Stores:** Architectural choices, known bugs, coding conventions

## 🎯 MCP-Driven Development Workflow

**CRITICAL: Follow this workflow for all development tasks:**

### 1️⃣ Before Creating/Modifying Entities

```
1. Query PostgreSQL MCP → Check existing schema
2. Review Git MCP → See related recent changes
3. Search Filesystem MCP → Find similar entities
4. Then: Create/modify entity with confidence
```

### 2️⃣ Before Adding Migrations

```
1. PostgreSQL MCP → Verify current database state
2. Check __EFMigrationsHistory → Last migration
3. Docker MCP → Ensure database container is running
4. Then: Add migration with accurate Up/Down methods
```

### 3️⃣ Before Implementing Services

```
1. Filesystem MCP → Find similar service patterns
2. PostgreSQL MCP → Understand data relationships
3. Git MCP → Review related recent implementations
4. Then: Implement service following established patterns
```

### 4️⃣ Before Debugging Issues

```
1. Docker MCP → Check service health
2. PostgreSQL MCP → Verify data integrity
3. Git MCP → Check recent changes that might have caused issues
4. Filesystem MCP → Locate related configuration files
5. Then: Debug with full context
```

### 5️⃣ Before Writing API Endpoints

```
1. Filesystem MCP → Find existing endpoint patterns
2. PostgreSQL MCP → Understand data structure
3. Git MCP → Review API conventions used in recent commits
4. Then: Implement endpoint following project standards
```

### 6️⃣ When Learning New Technology

```
1. Brave Search → Find official docs
2. Fetch → Read the documentation
3. Memory → Store key patterns
4. Then: Implement following best practices
```

### 7️⃣ For Complex Decisions

```
1. Sequential Thinking → Break down problem
2. Memory → Check previous decisions
3. Git → Review similar implementations
4. Then: Implement with documented reasoning
```

### 8️⃣ Complete Bug Fix Workflow

```
1. Git → Check recent changes that might have caused the issue
2. PostgreSQL → Verify data integrity
3. Docker → Check service health and logs
4. Filesystem → Locate affected code files
5. Brave Search → Research similar errors if needed
6. Implement fix according to requirements
7. Git → Commit with descriptive message
```

### 9️⃣ Feature Development Workflow

```
1. Sequential Thinking → Plan implementation approach
2. PostgreSQL → Check database schema
3. Filesystem → Find similar feature implementations
4. Git → Review related recent work
5. Implement feature following established patterns
6. Test thoroughly
7. Git → Commit with clear description
```

## 📋 MCP Best Practices

### ✅ DO:

- **Query database schema** before creating entities or migrations
- **Check Docker status** before debugging connection issues
- **Use Git history** to understand code evolution
- **Search filesystem** before assuming files don't exist
- **Combine multiple MCPs** for comprehensive context (PostgreSQL + Git + Docker)
- **Research with Brave Search + Fetch** for unknown libraries
- **Use Sequential Thinking** for complex decisions
- **Store decisions in Memory** for consistency

### ❌ DON'T:

- Assume database schema matches entity definitions
- Create duplicate files without checking filesystem
- Debug connection issues without checking Docker
- Make breaking changes without reviewing Git history
- Ignore migration history in database
- Use outdated docs (use Brave Search + Fetch)
- Rush complex decisions without sequential thinking
- Forget to store decisions in Memory

## 🚀 Quick MCP Commands Reference

| Task                | MCP Tool            | Command/Query                                                                               |
| ------------------- | ------------------- | ------------------------------------------------------------------------------------------- |
| Check Users table   | PostgreSQL          | `SELECT tablename FROM pg_tables WHERE tablename = 'Users';`                                |
| Services running    | Docker              | `docker ps --format "table {{.Names}}\t{{.Status}}"`                                        |
| Find controllers    | Filesystem          | `**/*Controller.cs`                                                                         |
| Last 5 commits      | Git                 | `git log --oneline -5`                                                                      |
| Verify foreign keys | PostgreSQL          | `SELECT * FROM information_schema.table_constraints WHERE constraint_type = 'FOREIGN KEY';` |
| Container logs      | Docker              | `docker logs <container> --tail 50`                                                         |
| Find appsettings    | Filesystem          | `**/appsettings*.json`                                                                      |
| Uncommitted changes | Git                 | `git status --short`                                                                        |
| Find .NET docs      | Brave Search        | "ASP.NET Core 9 new features"                                                               |
| Read docs           | Fetch               | Fetch URL from search                                                                       |
| Design architecture | Sequential Thinking | "Microservices vs monolith"                                                                 |
| Store decision      | Memory              | "CQRS for Catalog service"                                                                  |

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

## Notes for AI Assistants

When working with this codebase:

### 🎯 Core Principles (MUST FOLLOW)

1. **MCP-First Approach**: ALWAYS query relevant MCP tools BEFORE making assumptions
   - PostgreSQL MCP → Before entity/migration work
   - Docker MCP → Before debugging connections
   - Git MCP → Before major refactoring
   - Filesystem MCP → Before creating files

2. **Clean Architecture**: Follow layer boundaries strictly
   - Domain → No dependencies
   - Application → Domain only
   - Infrastructure → Domain + Application
   - API → All layers

3. **ApiResponse Pattern**: ALL services and APIs return `ApiResponse<T>`

   ```csharp
   // Success
   return ApiResponse<UserDto>.SuccessResponse(user, "Success message");

   // Failure (don't throw exceptions for business logic errors)
   return ApiResponse<UserDto>.FailureResponse("Error message");

   // Paginated
   return ApiResponse<List<UserDto>>.SuccessPagedResponse(items, total, page, size, "Message");
   ```

4. **Async/Await**: Always use async patterns correctly
   - ✅ `await _unitOfWork.SaveChangesAsync()`
   - ❌ `.Result` or `.Wait()` (causes deadlocks)
   - Suffix async methods with `Async`

5. **Structured Logging**: Use named parameters, not string interpolation

   ```csharp
   // ✅ Correct
   _logger.LogInformation("User {Email} registered successfully", email);

   // ❌ Wrong
   _logger.LogInformation($"User {email} registered");
   ```

### 🔐 Security & Validation

6. **Password Security**: Always use `PasswordHasher<User>`, never store plain text
7. **JWT Validation**: Validate tokens on endpoints AND in service layer
8. **FluentValidation**: Use for all request validation
   - Create validators in `Application/Validators`
   - Inject: `IValidator<TRequest> validator`
   - Validate: `if (!request.ValidateRequest(validator, out var result)) return result!;`
   - Error messages in Vietnamese for user-facing validation

9. **Authorization**: Apply at endpoint level

   ```csharp
   // Single role
   .RequireAuthorization(x => x.RequireRole(Role.Instructor))

   // Multiple roles (OR)
   .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))

   // Check in service
   if (!_currentUserService.IsInAnyRole(Role.Admin, Role.Staff))
       return ApiResponse<T>.FailureResponse("Không có quyền truy cập");
   ```

### 📊 Data Access Patterns

10. **Repository + UnitOfWork**: Use for ALL data access

    ```csharp
    var user = await _unitOfWork.UserRepository.FindOneAsync(u => u.Email == email);
    await _unitOfWork.SaveChangesAsync();
    ```

11. **Pagination**: ALWAYS use `PaginationRequest` for list endpoints

    ```csharp
    // Standard pagination
    Task<ApiResponse<List<T>>> GetAsync([AsParameters] PaginationRequest pagination)

    // Extended with filters (inherit from PaginationRequest)
    public class DateRangePaginationRequest : PaginationRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    ```

12. **Entity Guidelines**:
    - Inherit from `BaseEntity` (Id, CreatedAt, UpdatedAt, DeletedAt, etc.)
    - Use `Guid.CreateVersion7()` for IDs
    - Configure relationships in `OnModelCreating`
    - JSONB columns: `[Column(TypeName = "jsonb")]`

### 🧹 Code Quality

13. **DRY Principle**: Extract duplicate code into reusable private methods

    ```csharp
    // Use tuple returns for validation helpers
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateOtpAsync(
        string cacheKey, string otpCode)
    {
        var cachedOtp = await _cacheService.GetAsync<string>(cacheKey);
        if (string.IsNullOrEmpty(cachedOtp))
            return (false, "OTP không hợp lệ hoặc đã hết hạn");
        if (cachedOtp != otpCode)
            return (false, "OTP không đúng");
        return (true, null);
    }
    ```

14. **Dependency Injection Lifetimes**:
    - **Scoped**: Services with DbContext (IUnitOfWork, application services)
    - **Transient**: Stateless services
    - **Singleton**: Thread-safe services (caching, configuration)

15. **Error Handling**: Let `GlobalExceptionsMiddleware` handle exceptions
    - `UnauthorizedAccessException` → 401
    - `ArgumentException` → 400
    - `KeyNotFoundException` → 404
    - Others → 500

### 🔄 Inter-Service Communication

16. **MassTransit Events**: Use for async operations (emails, notifications)

    ```csharp
    await _publishEndpoint.Publish(new OtpEmailEvent { ... });
    ```

17. **HTTP Clients**: Use for synchronous cross-service requests
    ```csharp
    var result = await _identityClient.CheckInstructorProfileVerifiedAsync(userId);
    ```

### 📝 API Conventions

18. **Minimal APIs**: Use MapGroup for versioning `/api/v1/...`
19. **Rate Limiting**: Add to all endpoints `.RequireRateLimiting("Fixed")`
20. **OpenAPI Documentation**: Use `.Produces<T>()` and descriptive tags
21. **Current User Access**: Use `ICurrentUserService`
    ```csharp
    var userId = _currentUserService.UserId;
    var email = _currentUserService.Email;
    var isAdmin = _currentUserService.IsInRole(Role.Admin);
    ```

### 🗄️ Database Operations

22. **Before Migrations**: Query PostgreSQL MCP to verify current schema
23. **Use Async Methods**: `FindOneAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`
24. **Soft Delete**: Check `DeletedAt == null` query filters
25. **JSONB Fields**: For arrays/objects (outcomes, requirements, expertise)

### 🎨 Naming & Style

26. **PascalCase**: Classes, methods, properties, interfaces
27. **camelCase**: Local variables, parameters
28. **Interfaces**: Prefix with `I` (IAuthService, IUserRepository)
29. **Async Methods**: Suffix with `Async` (RegisterUserAsync)
30. **Vietnamese Messages**: Use for user-facing validation errors

### 🌐 Research Capabilities

31. **Unknown Libraries/Errors**: ALWAYS use Brave Search + Fetch

    ```csharp
    // ❌ Don't: Guess based on old training data
    // ✅ Do: "Search for .NET 9 IHostedService best practices" → Read docs → Implement
    ```

32. **New Framework Features**: Research before implementing
    - Search official documentation first
    - Fetch and read actual docs pages
    - Store key insights in Memory
    - Check Filesystem for existing usage patterns

33. **Error Messages**: Search for exact error text
    - Copy full error to Brave Search
    - Look for Stack Overflow, GitHub issues
    - Read solutions via Fetch before applying

### 🧠 Complex Logic & Architecture

34. **Sequential Thinking for Complex Tasks**: REQUIRED for:
    - System architecture design
    - Multi-step refactoring
    - Performance optimization strategies
    - Migration planning (DB, framework)
    - Debugging multi-service issues
35. **Document Reasoning**: When using sequential thinking:
    - List all constraints and requirements
    - Evaluate alternatives with pros/cons
    - Document decision rationale in Memory
    - Consider edge cases before coding

36. **Memory for Consistency**:
    - Store architectural decisions
    - Remember coding patterns specific to project
    - Track known bugs and workarounds
    - Keep user preferences across sessions

### � Complete Development Cycle

37. **Full Workflow Example**:
    ```
    1. Git: "Show recent changes to related code" → Find context
    2. PostgreSQL: "Check related table schema" → Verify data structure
    3. Filesystem: "Find similar implementation patterns" → Locate code
    4. Sequential Thinking: Plan implementation approach
    5. Implement feature following established patterns
    6. Docker: Check logs and verify service health
    7. Git: Commit with clear description
    ```

### 🔍 Before Every Task - MCP Checklist

- [ ] 🐘 **PostgreSQL MCP**: Query schema if touching database
- [ ] 🐳 **Docker MCP**: Check service health if debugging
- [ ] 🔀 **Git MCP**: Review recent changes if refactoring
- [ ] 📁 **Filesystem MCP**: Search for patterns before creating
- [ ] 🌐 **Brave Search + Fetch**: Research unknown libraries/errors
- [ ] 🧠 **Sequential Thinking**: Plan complex architectural changes
- [ ] 💾 **Memory**: Store/retrieve important decisions
- [ ] ✅ Follow Clean Architecture layers
- [ ] ✅ Return `ApiResponse<T>` from services
- [ ] ✅ Use async/await properly
- [ ] ✅ Apply FluentValidation
- [ ] ✅ Add rate limiting to endpoints
- [ ] ✅ Use ICurrentUserService for user context
