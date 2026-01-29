using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.Instructors
{
    public class UpdateInstructorProfileRequest
    {
        public string? Bio { get; set; } = string.Empty;
        public string? Headline { get; set; } = string.Empty;
        public List<string>? ExpertiseAreas { get; set; } = [];
        public List<EducationInfo>? Education { get; set; } = [];
        public List<WorkInfo>? WorkExperience { get; set; }
        public SocialInfo? SocialLinks { get; set; }
        public BankInfo? BankInfo { get; set; }
        public List<string>? TeachingLanguages { get; set; }
        public string? IntroVideoUrl { get; set; }
        public List<CertificateInfo>? Certificates { get; set; }
    }
}