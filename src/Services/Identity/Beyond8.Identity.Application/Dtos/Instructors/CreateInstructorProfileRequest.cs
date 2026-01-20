using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class CreateInstructorProfileRequest
{
    public string Bio { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public List<string> ExpertiseAreas { get; set; } = [];
    public List<EducationInfo> Education { get; set; } = [];
    public List<WorkInfo>? WorkExperience { get; set; }
    public SocialInfo? SocialLinks { get; set; }
    public string BankInfo { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public List<IdentityInfo> IdentityDocuments { get; set; } = [];
    public List<CertificateInfo>? Certificates { get; set; }
}
