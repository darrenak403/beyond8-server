# 🏗️ Architecture & Standards - Beyond8 Server

## Technology Stack

- **Framework**: ASP.NET Core 9.0 (with .NET Aspire)
- **Architecture**: Clean Architecture with Microservices
- **Database**: PostgreSQL
- **Caching**: Redis (via ICacheService)
- **Messaging**: RabbitMQ with MassTransit
- **Authentication**: JWT tokens
- **API Style**: Minimal APIs
- **ORM**: Entity Framework Core
- **Notifications**: Firebase Cloud Messaging (FCM)

---

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
│   │   ├── Integration/                  # Media, AI, Notifications
│   │   │   ├── Beyond8.Integration.Api/
│   │   │   ├── Beyond8.Integration.Application/
│   │   │   ├── Beyond8.Integration.Domain/
│   │   │   └── Beyond8.Integration.Infrastructure/
│   │   ├── Catalog/                      # Course catalog management
│   │   │   ├── Beyond8.Catalog.Api/
│   │   │   ├── Beyond8.Catalog.Application/
│   │   │   ├── Beyond8.Catalog.Domain/
│   │   │   └── Beyond8.Catalog.Infrastructure/
│   │   ├── Sale/                         # Orders, Payments, Payouts
│   │   ├── Learning/                     # Enrollments, Progress tracking
│   │   └── Assessment/                   # Quizzes, Assignments
├── shared/
│   ├── Beyond8.Common/                   # Common utilities and shared code
│   └── Beyond8.DatabaseMigrationHelpers/ # Database migration helpers
└── beyond8-server.sln
```

---

## Clean Architecture Layers

Each service follows Clean Architecture with four distinct layers (strictly enforced):

### 1. Domain Layer (`*.Domain`)

- **Purpose**: Core business logic and entities
- **Contains**:
  - Domain entities (inherit from `BaseEntity`)
  - Repository interfaces
  - Domain enums
  - Business rules
- **Dependencies**: None (completely independent)
- **Rule**: NEVER reference Application, Infrastructure, or API layers

### 2. Application Layer (`*.Application`)

- **Purpose**: Business logic and use cases
- **Contains**:
  - DTOs (Data Transfer Objects)
  - Service interfaces and implementations
  - Mapping extensions
  - Validation logic (FluentValidation)
- **Dependencies**: Domain layer only
- **Rule**: NO database implementation, NO HTTP concerns

### 3. Infrastructure Layer (`*.Infrastructure`)

- **Purpose**: External concerns and data persistence
- **Contains**:
  - DbContext implementations
  - Repository implementations
  - External service integrations
  - Migration configurations
- **Dependencies**: Domain and Application layers
- **Rule**: This is the ONLY layer that talks to databases, file systems, external APIs

### 4. API Layer (`*.Api`)

- **Purpose**: HTTP endpoints and API configuration
- **Contains**:
  - Minimal API endpoints
  - Middleware configuration
  - OpenAPI/Swagger setup
  - Program.cs configuration
- **Dependencies**: Application and Infrastructure layers
- **Rule**: Controllers/Endpoints should be thin, only handle HTTP concerns

---

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
builder.Services.AddSingleton<ICacheService, CacheService>();
```

---

## Important Conventions

### API Endpoints

- Use Minimal APIs with MapGroup for versioning: `/api/v1/...`
- Create static extension methods for endpoint mapping (e.g., `MapAuthApi()`)
- Specify authorization explicitly: `RequireAuthorization()` or `AllowAnonymous()`
- Document with `.Produces<T>()` for OpenAPI generation
- Add descriptive tags and names for documentation
- Apply rate limiting: `.RequireRateLimiting("Fixed")`

### Pagination

- **ALWAYS use PaginationRequest** for endpoints that return lists of data
- Use `[AsParameters]` attribute to bind pagination parameters from query string
- For endpoints requiring additional filters, create a new DTO that **inherits from PaginationRequest**
- Return paginated responses using `ApiResponse<List<T>>.SuccessPagedResponse()`

**Example:**

```csharp
// Standard Pagination
private static async Task<IResult> GetUsers(
    [FromServices] IUserService userService,
    [AsParameters] PaginationRequest pagination)
{
    var result = await userService.GetUsersAsync(pagination);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
}

// Extended Pagination with filters
public class DateRangePaginationRequest : PaginationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
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
// ✅ Good
logger.LogInformation("User registered successfully: {Email}", request.Email);

// ❌ Bad
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

**Example:**

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

---

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

**Retry Policy:**

- Exponential backoff: 5 retries
- Min interval: 2 seconds
- Max interval: 30 seconds

### Inter-Service Communication

Services communicate via:

1. **HTTP Clients**: For synchronous requests (e.g., `IIdentityClient`)
2. **MassTransit Events**: For asynchronous operations (emails, notifications)

---

## Database

- **Type**: PostgreSQL
- **Connection Strings**: Stored in appsettings.json
- **Migrations**: Applied on startup in Development environment
- **Context Factory**: Used for creating contexts in migrations
- **Each Service**: Has its own database (database-per-service pattern)

---

## Common Shared Projects

### Beyond8.Common

Contains shared utilities, constants, and helper classes used across all services.

### Beyond8.DatabaseMigrationHelpers

Contains helpers and extensions for database migration management.

---

## Configuration

- **appsettings.json**: Main configuration file
- **appsettings.Development.json**: Development overrides
- **User Secrets**: For sensitive data in development
- **Environment Variables**: For sensitive data in production

Connection string constants defined in `Const` class (e.g., `Const.IdentityServiceDatabase`).

---

## Naming Conventions

- **PascalCase**: Classes, methods, properties, interfaces

  ```csharp
  public class UserService : IUserService
  public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
  public string FullName { get; set; }
  ```

- **camelCase**: Local variables, parameters, private fields

  ```csharp
  var userRepository = _unitOfWork.UserRepository;
  public async Task RegisterAsync(RegisterRequest request)
  private readonly ILogger<AuthService> _logger;
  ```

- **Interface Prefix**: Always start with `I`

  ```csharp
  IAuthService, IUserRepository, ICurrentUserService
  ```

- **Async Suffix**: All async methods end with `Async`
  ```csharp
  RegisterUserAsync, GetOrderByIdAsync, SaveChangesAsync
  ```

---

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
- [ ] Code follows consistent naming conventions
- [ ] Database operations use async methods only
- [ ] No duplicate code - extract common logic into reusable methods
