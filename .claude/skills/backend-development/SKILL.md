---
name: backend-development
description: This skill guides backend development for Beyond8 microservices. Use when designing APIs, implementing features, reviewing code quality, or making architectural decisions. Covers API design patterns, Clean Architecture layers, validation, error handling, and development mindset for ASP.NET Core with .NET Aspire.
---

# Backend Development Guide

Comprehensive guide for developing backend features in Beyond8 microservices system.

## Development Workflow

### 1. Understand Requirements
- Clarify business requirements before coding
- Identify affected services (Identity, Catalog, Integration, Assessment)
- Map data flow between services

### 2. Design API First
- Define endpoints, request/response DTOs
- Follow RESTful conventions
- Document with OpenAPI annotations
- Reference: `references/api-design.md`

### 3. Implement with Clean Architecture
- Domain → Application → Infrastructure → API
- Never skip layers or create circular dependencies
- Reference: `references/architecture.md`

### 4. Ensure Code Quality
- Validation with FluentValidation
- Consistent error handling with ApiResponse<T>
- Structured logging
- Reference: `references/code-quality.md`

### 5. Review Checklist
Before completing any feature:
- [ ] DTOs have Vietnamese validation messages
- [ ] Services return ApiResponse<T>, not throw exceptions
- [ ] All endpoints have proper authorization
- [ ] Rate limiting applied: `.RequireRateLimiting("Fixed")`
- [ ] Async methods use `Async` suffix
- [ ] No duplicate code - extract common logic

## Quick Reference

### API Response Patterns
```csharp
// Success
ApiResponse<T>.SuccessResponse(data, "Thành công")

// Failure
ApiResponse<T>.FailureResponse("Lỗi message")

// Paginated
ApiResponse<List<T>>.SuccessPagedResponse(items, total, page, size, "Thành công")
```

### Service Registration
```csharp
builder.Services.AddScoped<IService, Service>();     // Database-dependent
builder.Services.AddTransient<IHelper, Helper>();    // Stateless
builder.Services.AddSingleton<ICache, Cache>();      // Thread-safe only
```

### Authorization
```csharp
.RequireAuthorization()                                    // Any authenticated
.RequireAuthorization(x => x.RequireRole(Role.Admin))      // Single role
.RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))  // Multiple roles
```

## Resources

### References
- `references/api-design.md` - API design patterns and conventions
- `references/architecture.md` - Clean Architecture implementation guide
- `references/code-quality.md` - Code quality standards and patterns
- `references/mindset.md` - Development mindset and best practices
