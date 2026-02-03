# Clean Architecture Guide

## Layer Structure

```
Service/
├── Domain/           # Core business logic (no dependencies)
├── Application/      # Use cases, DTOs, services
├── Infrastructure/   # External concerns, data access
└── Api/              # HTTP endpoints, configuration
```

## Layer Rules

### Domain Layer
**Purpose**: Core business entities and rules
**Dependencies**: NONE

```csharp
// Entities inherit from BaseEntity
public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public Guid InstructorId { get; set; }

    // Navigation properties
    public virtual ICollection<Section> Sections { get; set; } = [];
}

// Repository interfaces
public interface ICourseRepository : IGenericRepository<Course>
{
    Task<(List<Course>, int)> SearchCoursesAsync(...);
}

// Domain enums
public enum CourseStatus { Draft, Published, Archived }
```

### Application Layer
**Purpose**: Business logic orchestration
**Dependencies**: Domain only

```csharp
// DTOs
public class CourseResponse { ... }
public class CreateCourseRequest { ... }

// Service interfaces
public interface ICourseService
{
    Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request);
    Task<ApiResponse<List<CourseResponse>>> GetAllAsync(PaginationRequest pagination);
}

// Service implementations
public class CourseService(
    IUnitOfWork unitOfWork,
    ILogger<CourseService> logger) : ICourseService
{
    public async Task<ApiResponse<CourseResponse>> CreateAsync(CreateCourseRequest request)
    {
        // Business logic here
    }
}

// Mappings
public static class CourseMappings
{
    public static CourseResponse ToResponse(this Course course) => new()
    {
        Id = course.Id,
        Title = course.Title
    };
}

// Validators
public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest> { }
```

### Infrastructure Layer
**Purpose**: External concerns implementation
**Dependencies**: Domain, Application

```csharp
// DbContext
public class CatalogDbContext : BaseDbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
}

// Repository implementations
public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public async Task<(List<Course>, int)> SearchCoursesAsync(...)
    {
        // EF Core queries
    }
}

// Unit of Work
public class UnitOfWork : BaseUnitOfWork, IUnitOfWork
{
    public ICourseRepository CourseRepository { get; }
}

// External service clients
public class IdentityClient : IIdentityClient { }
```

### API Layer
**Purpose**: HTTP interface
**Dependencies**: Application, Infrastructure

```csharp
// Program.cs
builder.AddApplicationServices();  // DI setup
app.UseApplicationServices();      // Middleware

// Apis
public static class CourseApis
{
    public static IEndpointRouteBuilder MapCourseApi(this IEndpointRouteBuilder builder)
    {
        // Minimal API endpoints
    }
}
```

## Data Flow Example

```
Request → API Layer → Application Service → Repository → Database
                          ↓
                    Domain Entity
                          ↓
Response ← API Layer ← Application Service (maps to DTO)
```

## Dependency Injection

### Registration Order
```csharp
// 1. Infrastructure
services.AddDbContext<CatalogDbContext>();
services.AddScoped<IUnitOfWork, UnitOfWork>();

// 2. Application Services
services.AddScoped<ICourseService, CourseService>();

// 3. Validators
services.AddValidatorsFromAssemblyContaining<CreateCourseRequest>();
```

### Service Lifetimes
| Lifetime | Use Case |
|----------|----------|
| Scoped | Services with DbContext |
| Transient | Stateless utilities |
| Singleton | Thread-safe caches, configs |

## Inter-Service Communication

### Synchronous (HTTP)
```csharp
// Client interface
public interface IIdentityClient
{
    Task<ApiResponse<bool>> CheckInstructorVerifiedAsync(Guid userId);
}

// Usage in service
var verified = await identityClient.CheckInstructorVerifiedAsync(userId);
if (!verified.Data) return ApiResponse<T>.FailureResponse("Instructor not verified");
```

### Asynchronous (MassTransit/RabbitMQ)
```csharp
// Publish event
await publishEndpoint.Publish(new CourseApprovedEvent
{
    CourseId = course.Id,
    InstructorId = course.InstructorId
});

// Consumer
public class CourseApprovedConsumer : IConsumer<CourseApprovedEvent>
{
    public async Task Consume(ConsumeContext<CourseApprovedEvent> context)
    {
        // Handle event
    }
}
```

## Common Patterns

### Repository Pattern
```csharp
// Generic operations via IGenericRepository<T>
await unitOfWork.CourseRepository.AddAsync(course);
await unitOfWork.CourseRepository.UpdateAsync(id, course);
await unitOfWork.SaveChangesAsync();

// Custom queries in specific repository
var courses = await unitOfWork.CourseRepository.SearchCoursesAsync(...);
```

### Unit of Work
```csharp
// Single transaction
var course = new Course { ... };
await unitOfWork.CourseRepository.AddAsync(course);

var section = new Section { CourseId = course.Id, ... };
await unitOfWork.SectionRepository.AddAsync(section);

await unitOfWork.SaveChangesAsync(); // Commits all changes
```
