using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Dtos.Courses;

/// <summary>
/// Request DTO for PostgreSQL Full-Text Search on courses.
/// Results are automatically ranked by relevance (Title > ShortDescription > Description > InstructorName).
/// Supports Vietnamese diacritics search (e.g., "lap trinh" matches "Lập trình").
/// </summary>
public class FullTextSearchRequest : PaginationRequest
{
    public string? Keyword { get; set; } = string.Empty;
}
