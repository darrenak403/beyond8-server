namespace Beyond8.Learning.Application.Dtos.Certificates;

public class UpdateCertificateEligibilityConfigRequest
{
    public decimal? QuizAverageMinPercent { get; set; }

    public decimal? AssignmentAverageMinPercent { get; set; }
}
