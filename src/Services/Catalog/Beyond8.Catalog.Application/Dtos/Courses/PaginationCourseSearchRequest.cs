using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Dtos.Courses;

/// <summary>
/// Pagination request for public course search (always returns Published & Active courses)
/// </summary>
public class PaginationCourseSearchRequest : PaginationRequest
{
    public string? Keyword { get; set; }
    public string? CategoryName { get; set; }
    public string? InstructorName { get; set; }
    public CourseLevel? Level { get; set; }
    public string? Language { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public int? MinStudents { get; set; }
    public bool? IsDescendingPrice { get; set; }
    public bool? IsRandom { get; set; }
}

/// <summary>
/// Pagination request for instructor course search (supports status filter for own courses)
/// </summary>
public class PaginationCourseInstructorSearchRequest : PaginationCourseSearchRequest
{
    public CourseStatus? Status { get; set; }
}

/// <summary>
/// Pagination request for admin course search (supports all status and active filters)
/// </summary>
public class PaginationCourseAdminSearchRequest : PaginationCourseSearchRequest
{
    public CourseStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}
