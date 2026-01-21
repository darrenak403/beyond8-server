using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.JSONFields;

public class UpdateInstructorProfileRequest
{
    public string? Bio { get; set; } = string.Empty;
    public string? Headline { get; set; } = string.Empty;
    public List<string>? ExpertiseAreas { get; set; } = [];
    public List<EducationInfo>? Education { get; set; } = [];
    public List<WorkInfo>? WorkExperience { get; set; }
    public SocialInfo? SocialLinks { get; set; }
    public List<IdentityInfo>? IdentityDocuments { get; set; } = [];
    public List<CertificateInfo>? Certificates { get; set; }
}