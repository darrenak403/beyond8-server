using System;
using Beyond8.Common.Utilities;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class PaginationInstructorRequest : PaginationRequest
{
    public string? Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? HeadLine { get; set; }
    public string? ExpertiseAreas { get; set; }
    public string? SchoolName { get; set; }
    // Work Experience search
    public string? CompanyName { get; set; }

    // public decimal? MinAvgRating { get; set; }
    // public decimal? MaxAvgRating { get; set; }
    // public int? MinTotalStudents { get; set; }
    // public int? MinTotalCourses { get; set; }
}
