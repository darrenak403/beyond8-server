# EF Core Queries Guide

## Basic Queries

### Find by ID
```csharp
var course = await unitOfWork.CourseRepository.FindByIdAsync(id);
// Returns null if not found
```

### Find with Predicate
```csharp
var course = await unitOfWork.CourseRepository.FindOneAsync(c => c.Slug == slug);
var user = await unitOfWork.UserRepository.FindOneAsync(u => u.Email == email);
```

### Get All with Filter
```csharp
var courses = await unitOfWork.CourseRepository
    .AsQueryable()
    .Where(c => c.Status == CourseStatus.Published)
    .Where(c => c.IsActive)
    .ToListAsync();
```

## Include Related Data

### Single Level Include
```csharp
var course = await unitOfWork.CourseRepository
    .AsQueryable()
    .Include(c => c.Category)
    .Include(c => c.Documents)
    .FirstOrDefaultAsync(c => c.Id == courseId);
```

### Nested Include (ThenInclude)
```csharp
var course = await unitOfWork.CourseRepository
    .AsQueryable()
    .Include(c => c.Sections)
        .ThenInclude(s => s.Lessons)
            .ThenInclude(l => l.Video)
    .Include(c => c.Sections)
        .ThenInclude(s => s.Lessons)
            .ThenInclude(l => l.Text)
    .FirstOrDefaultAsync(c => c.Id == courseId);
```

### Filtered Include (EF Core 5+)
```csharp
var course = await unitOfWork.CourseRepository
    .AsQueryable()
    .Include(c => c.Sections.Where(s => s.IsPublished))
        .ThenInclude(s => s.Lessons.Where(l => l.IsPublished))
    .FirstOrDefaultAsync(c => c.Id == courseId);
```

## Ordering

### Single Order
```csharp
var courses = await query
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();
```

### Multiple Orders
```csharp
var courses = await query
    .OrderBy(c => c.Status)
    .ThenByDescending(c => c.AvgRating)
    .ThenBy(c => c.Title)
    .ToListAsync();
```

### Dynamic Ordering
```csharp
var query = unitOfWork.CourseRepository.AsQueryable();

query = isDescending
    ? query.OrderByDescending(c => c.CreatedAt)
    : query.OrderBy(c => c.CreatedAt);
```

## Pagination

### Standard Pattern
```csharp
public async Task<(List<Course>, int)> GetPagedAsync(int page, int size)
{
    var query = unitOfWork.CourseRepository
        .AsQueryable()
        .Where(c => c.IsActive);

    var totalCount = await query.CountAsync();

    var items = await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * size)
        .Take(size)
        .ToListAsync();

    return (items, totalCount);
}
```

### With Search
```csharp
var query = unitOfWork.CourseRepository.AsQueryable();

if (!string.IsNullOrWhiteSpace(keyword))
{
    var searchTerm = keyword.ToLower();
    query = query.Where(c =>
        c.Title.ToLower().Contains(searchTerm) ||
        c.Description.ToLower().Contains(searchTerm));
}

if (status.HasValue)
    query = query.Where(c => c.Status == status.Value);

if (level.HasValue)
    query = query.Where(c => c.Level == level.Value);
```

## Projections (Select)

### Anonymous Type
```csharp
var summaries = await query
    .Select(c => new
    {
        c.Id,
        c.Title,
        c.ThumbnailUrl,
        c.Price,
        c.AvgRating
    })
    .ToListAsync();
```

### DTO Projection
```csharp
var responses = await query
    .Select(c => new CourseSimpleResponse
    {
        Id = c.Id,
        Title = c.Title,
        ThumbnailUrl = c.ThumbnailUrl,
        Price = c.Price,
        AvgRating = c.AvgRating,
        InstructorName = c.InstructorName,
        CategoryName = c.Category.Name
    })
    .ToListAsync();
```

## Aggregations

### Count
```csharp
var totalCourses = await query.CountAsync();
var publishedCount = await query.CountAsync(c => c.Status == CourseStatus.Published);
```

### Sum/Average
```csharp
var totalStudents = await query.SumAsync(c => c.TotalStudents);
var avgRating = await query.AverageAsync(c => c.AvgRating ?? 0);
```

### Any/All
```csharp
var hasPublished = await query.AnyAsync(c => c.Status == CourseStatus.Published);
var allActive = await query.AllAsync(c => c.IsActive);
```

### GroupBy
```csharp
var coursesByLevel = await query
    .GroupBy(c => c.Level)
    .Select(g => new
    {
        Level = g.Key,
        Count = g.Count(),
        AvgPrice = g.Average(c => c.Price)
    })
    .ToListAsync();
```

## Tracking Behavior

### No Tracking (Read-only)
```csharp
// Better performance for read-only queries
var courses = await query
    .AsNoTracking()
    .ToListAsync();
```

### With Tracking (For updates)
```csharp
// Default behavior - entities are tracked
var course = await query.FirstOrDefaultAsync(c => c.Id == id);
course.Title = "New Title";
await unitOfWork.SaveChangesAsync(); // Changes are saved
```

## Raw SQL (When Needed)

### FromSqlRaw
```csharp
var courses = await context.Courses
    .FromSqlRaw("SELECT * FROM \"Courses\" WHERE \"Price\" > {0}", minPrice)
    .ToListAsync();
```

### ExecuteSqlRaw
```csharp
await context.Database.ExecuteSqlRawAsync(
    "UPDATE \"Courses\" SET \"TotalStudents\" = \"TotalStudents\" + 1 WHERE \"Id\" = {0}",
    courseId);
```

## Common Patterns in Beyond8

### Check Existence
```csharp
var exists = await unitOfWork.CourseRepository
    .AsQueryable()
    .AnyAsync(c => c.Slug == slug && c.Id != excludeId);
```

### Get with Category
```csharp
var course = await unitOfWork.CourseRepository
    .AsQueryable()
    .Include(c => c.Category)
    .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);
```

### Search with Multiple Criteria
```csharp
var query = unitOfWork.CourseRepository
    .AsQueryable()
    .Where(c => c.Status == CourseStatus.Published && c.IsActive);

if (!string.IsNullOrWhiteSpace(keyword))
    query = query.Where(c => c.Title.Contains(keyword));

if (categoryId.HasValue)
    query = query.Where(c => c.CategoryId == categoryId);

if (minPrice.HasValue)
    query = query.Where(c => c.Price >= minPrice);

if (maxPrice.HasValue)
    query = query.Where(c => c.Price <= maxPrice);

if (minRating.HasValue)
    query = query.Where(c => c.AvgRating >= minRating);
```
