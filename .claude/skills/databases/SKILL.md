---
name: databases
description: Database operations for Beyond8 microservices using PostgreSQL with Entity Framework Core. Use when designing database schemas, writing EF Core queries and LINQ, creating migrations, optimizing performance, working with repositories and Unit of Work pattern, or managing database-per-service architecture.
license: MIT
---

# Databases Skill - Beyond8

Guide for working with PostgreSQL databases through Entity Framework Core in Beyond8 microservices.

## Architecture Overview

Beyond8 uses **database-per-service** pattern:
- `IdentityDb` - Users, authentication, instructors, subscriptions
- `CatalogDb` - Courses, sections, lessons, categories
- `IntegrationDb` - Media files, notifications, AI usage
- `AssessmentDb` - Quizzes, questions, attempts

## EF Core Patterns

### DbContext Structure
```csharp
public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : BaseDbContext(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Lesson> Lessons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete filter
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        // Relationships
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasOne(l => l.Video)
                  .WithOne(v => v.Lesson)
                  .HasForeignKey<LessonVideo>(v => v.LessonId);
        });
    }
}
```

### Repository Pattern
```csharp
// Generic repository usage
await unitOfWork.CourseRepository.AddAsync(course);
await unitOfWork.CourseRepository.UpdateAsync(id, course);
await unitOfWork.CourseRepository.FindByIdAsync(id);
await unitOfWork.CourseRepository.FindOneAsync(c => c.Slug == slug);
await unitOfWork.SaveChangesAsync();

// Custom repository methods
var (courses, total) = await unitOfWork.CourseRepository.SearchCoursesAsync(
    pageNumber, pageSize, keyword, status, level);
```

### LINQ Queries
```csharp
// Basic query with filtering
var courses = await unitOfWork.CourseRepository
    .AsQueryable()
    .Where(c => c.Status == CourseStatus.Published && c.IsActive)
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();

// Include related data
var course = await unitOfWork.CourseRepository
    .AsQueryable()
    .Include(c => c.Category)
    .Include(c => c.Sections.Where(s => s.IsPublished))
        .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
    .FirstOrDefaultAsync(c => c.Id == courseId);

// Pagination
var query = unitOfWork.CourseRepository.AsQueryable()
    .Where(c => c.IsActive);
var total = await query.CountAsync();
var items = await query
    .Skip((page - 1) * size)
    .Take(size)
    .ToListAsync();
```

## References

### EF Core Operations
- **[references/efcore-queries.md](references/efcore-queries.md)** - LINQ queries, includes, filtering, projections
- **[references/efcore-migrations.md](references/efcore-migrations.md)** - Creating and applying migrations

### PostgreSQL
- **[references/postgresql-queries.md](references/postgresql-queries.md)** - Raw SQL when needed
- **[references/postgresql-performance.md](references/postgresql-performance.md)** - Query optimization, indexes
- **[references/postgresql-administration.md](references/postgresql-administration.md)** - Backups, maintenance

## Quick Reference

### Async Methods (Always Use)
```csharp
await context.SaveChangesAsync();
await repo.FindByIdAsync(id);
await repo.FindOneAsync(predicate);
await query.ToListAsync();
await query.FirstOrDefaultAsync();
await query.CountAsync();
await query.AnyAsync();
```

### Common Mistakes
```csharp
// Bad - N+1 query problem
foreach (var course in courses)
{
    var sections = course.Sections; // Lazy load each time!
}

// Good - Eager loading
var courses = await query
    .Include(c => c.Sections)
    .ToListAsync();

// Bad - Loading all then filtering
var all = await query.ToListAsync();
var filtered = all.Where(c => c.IsActive);

// Good - Filter in database
var filtered = await query
    .Where(c => c.IsActive)
    .ToListAsync();
```

### JSONB Columns (PostgreSQL)
```csharp
// Entity property
public string Outcomes { get; set; } // Stored as JSONB

// Querying JSONB
.Where(c => EF.Functions.JsonContains(c.Outcomes, "\"ASP.NET\""))

// Mapping in service
var outcomes = JsonSerializer.Deserialize<List<string>>(course.Outcomes);
```
