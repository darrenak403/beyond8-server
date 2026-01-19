using Beyond8.Identity.Domain.Enums;

public class RejectInstructorApplicationRequest
{
    public VerificationStatus VerificationStatus { get; set; }
    public string VerificationNotes { get; set; } = string.Empty;
}