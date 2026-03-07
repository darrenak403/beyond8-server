namespace Beyond8.Learning.Application.Dtos.Certificates;

public class CertificateVerificationResponse
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime CompletionDate { get; set; }
    public DateTime IssuedDate { get; set; }
    public bool IsValid { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}
