using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class InstructorProfileAdminResponse : InstructorProfileResponse
{
    public BankInfo? BankInfo { get; set; }
    public string? TaxId { get; set; }
    public List<IdentityInfo>? IdentityDocuments { get; set; }
    public string? VerificationNotes { get; set; }
    public Guid? VerifiedBy { get; set; }
}
