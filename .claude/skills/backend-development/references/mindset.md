# Development Mindset

## Core Principles

### 1. Understand Before Code
- Read existing code before modifying
- Understand the business requirement fully
- Ask clarifying questions early
- Map the data flow through services

### 2. Design First, Code Second
- Sketch API contracts before implementation
- Define DTOs and interfaces first
- Consider edge cases during design
- Plan for failure scenarios

### 3. Keep It Simple (KISS)
- Solve the current problem, not hypothetical future ones
- Three similar lines > premature abstraction
- Avoid over-engineering
- Question every added complexity

### 4. Consistency Over Cleverness
- Follow existing patterns in codebase
- Match naming conventions
- Use established error handling
- Maintain architectural boundaries

## Anti-Patterns to Avoid

### Over-Engineering
```csharp
// Bad - over-abstracted for one use case
public interface IProcessor<TInput, TOutput, TConfig> where TConfig : IConfig { }
public class CourseProcessor : IProcessor<Course, CourseDto, CourseConfig> { }

// Good - simple and direct
public class CourseService : ICourseService { }
```

### Premature Optimization
```csharp
// Bad - caching before proving it's needed
private readonly ConcurrentDictionary<Guid, Course> _cache = new();
public async Task<Course> GetAsync(Guid id) { ... complex caching logic ... }

// Good - simple first, optimize when measured
public async Task<Course> GetAsync(Guid id)
{
    return await unitOfWork.CourseRepository.FindByIdAsync(id);
}
```

### Feature Creep
```csharp
// Task: Add delete course endpoint

// Bad - adding unrequested features
- Added soft delete
- Added restore endpoint
- Added bulk delete
- Added delete scheduling
- Added delete notifications

// Good - exactly what was requested
- Added DELETE /courses/{id} endpoint
```

## Decision Framework

### When to Create Abstraction
✅ Do abstract when:
- Pattern repeats 3+ times
- Logic is complex and error-prone
- Testing requires isolation

❌ Don't abstract when:
- Only 1-2 usages exist
- Code is straightforward
- Abstraction adds more complexity

### When to Add Features
✅ Do add when:
- Explicitly requested
- Required for core functionality
- Prevents security issues

❌ Don't add when:
- "Might be useful later"
- "While I'm here..."
- Not in requirements

## Code Review Checklist

### Functionality
- [ ] Meets requirements exactly
- [ ] Handles edge cases
- [ ] Error messages are helpful (Vietnamese)

### Architecture
- [ ] Follows Clean Architecture layers
- [ ] No circular dependencies
- [ ] Appropriate service lifetime

### Quality
- [ ] No duplicate code
- [ ] Proper validation
- [ ] Structured logging
- [ ] Unit testable

### Security
- [ ] Authorization checked
- [ ] Input validated
- [ ] No sensitive data exposed

## Communication

### When Stuck
1. Re-read the requirement
2. Check existing similar code
3. Search codebase for patterns
4. Ask specific questions

### Asking Good Questions
```
Bad: "How should I implement this?"
Good: "For the course deletion, should I use soft delete (set DeletedAt)
       or hard delete? The Category service uses soft delete."
```

### Reporting Progress
```
Bad: "Working on it..."
Good: "Created CourseResponse DTO and validator. Next: implementing
       CourseService.CreateAsync method."
```

## Performance Mindset

### Database Queries
- Use pagination for lists
- Select only needed columns
- Include related data explicitly
- Avoid N+1 queries

### Async Operations
- Don't block async code
- Use cancellation tokens
- Batch where possible

### Measure First
- Don't optimize without metrics
- Profile before changing
- Compare before/after

## Learning from Mistakes

### Common Mistakes
| Mistake | Fix |
|---------|-----|
| Forgetting rate limiting | Add `.RequireRateLimiting("Fixed")` |
| Missing authorization | Review endpoint security |
| String interpolation in logs | Use structured logging |
| Returning exceptions | Return ApiResponse failures |

### Debugging Approach
1. Reproduce the issue
2. Check logs for errors
3. Trace the data flow
4. Isolate the problem
5. Fix and verify
6. Add test to prevent regression

## Growth Mindset

### Daily Habits
- Read code before modifying
- Follow existing patterns
- Ask when uncertain
- Review own code before PR

### Continuous Improvement
- Learn from code reviews
- Study codebase patterns
- Update documentation
- Share knowledge with team
