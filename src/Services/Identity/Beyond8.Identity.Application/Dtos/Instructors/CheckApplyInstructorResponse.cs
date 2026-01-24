using System;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class CheckApplyInstructorResponse
{
    public bool IsApplied { get; set; }
    public VerificationStatus VerificationStatus { get; set; }

}
