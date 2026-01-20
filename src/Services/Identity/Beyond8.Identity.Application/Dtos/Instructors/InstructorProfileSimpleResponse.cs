using System;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class InstructorProfileSimpleResponse
{
    public Guid Id { get; set; }
    public UserSimpleResponse User { get; set; } = null!;
    public string? Headline { get; set; }
    public List<string>? ExpertiseAreas { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal? AvgRating { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
}
