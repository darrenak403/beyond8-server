using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class InstructorProfileResponse
{
    public Guid Id { get; set; }
    public UserSimpleResponse User { get; set; } = null!;
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public List<string>? ExpertiseAreas { get; set; }
    public List<EducationInfo>? Education { get; set; }
    public List<WorkInfo>? WorkExperience { get; set; }
    public SocialInfo? SocialLinks { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal? AvgRating { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}