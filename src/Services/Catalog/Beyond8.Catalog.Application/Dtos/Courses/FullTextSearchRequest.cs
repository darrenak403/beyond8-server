using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class FullTextSearchRequest : PaginationRequest
{
    public string? Keyword { get; set; } = string.Empty;
    public bool ExcludeEnrolledCourses { get; set; } = false;
}
