namespace Beyond8.Learning.Application.Dtos.Certificates;

public class CertificateEligibilityConfigResponse
{
    public Guid CourseId { get; set; }
    public decimal? QuizAverageMinPercent { get; set; }
    public decimal? AssignmentAverageMinPercent { get; set; }
}
