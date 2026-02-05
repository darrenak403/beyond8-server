namespace Beyond8.Learning.Application.Dtos.Certificates;

public class CertificateSimpleResponse
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string VerificationHash { get; set; } = string.Empty;
}
