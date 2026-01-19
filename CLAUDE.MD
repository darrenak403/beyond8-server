# Beyond8 Server - Claude AI Context

## Project Overview

Beyond8 is a microservices-based ASP.NET Core application following Clean Architecture principles. The system is built using .NET Aspire for orchestration and consists of multiple services handling different business domains.

## Technology Stack

- **Framework**: ASP.NET Core (with .NET Aspire)
- **Architecture**: Clean Architecture with Microservices
- **Database**: PostgreSQL
- **Caching**: Redis (via ICacheService)
- **Authentication**: JWT tokens
- **API Style**: Minimal APIs
- **ORM**: Entity Framework Core

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
│   │   └── Integration/                  # Integration service
│   │       ├── Beyond8.Integration.Api/
│   │       ├── Beyond8.Integration.Application/
│   │       ├── Beyond8.Integration.Domain/
│   │       └── Beyond8.Integration.Infrastructure/
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
Handles authentication and user management:
- User registration and login
- JWT token generation
- Password hashing and verification
- User profile management

### Integration Service
Handles external integrations and third-party service connections.

## Database

- **Type**: PostgreSQL
- **Connection Strings**: Stored in appsettings.json
- **Migrations**: Applied on startup in Development environment
- **Context Factory**: Used for creating contexts in migrations

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
- **Current Branch**: `cranky-beaver`
- **Worktree Path**: `C:\Users\hoade\.claude-worktrees\beyond8-server\cranky-beaver`
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

// Validation
[Required(ErrorMessage = "Error message")]
```

## Additional Documentation

For detailed ASP.NET Core best practices and coding standards, refer to:
- `.cursor/skills/asp-rules/SKILL.md` - Comprehensive ASP.NET Core guidelines
- `.github/copilot-instructions.md` - GitHub Copilot instructions

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
