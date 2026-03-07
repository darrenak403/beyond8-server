using Beyond8.Learning.Application.Dtos.Certificates;
using Beyond8.Learning.Domain.Entities;

namespace Beyond8.Learning.Application.Mappings;

public static class CertificateMappings
{
    public static CertificateVerificationResponse ToVerificationResponse(this Certificate entity)
    {
        return new CertificateVerificationResponse
        {
            Id = entity.Id,
            CertificateNumber = entity.CertificateNumber,
            StudentName = entity.StudentName,
            CourseTitle = entity.CourseTitle,
            InstructorName = entity.InstructorName,
            CompletionDate = entity.CompletionDate,
            IssuedDate = entity.IssuedDate,
            IsValid = entity.IsValid,
            RevokedAt = entity.RevokedAt,
            RevocationReason = entity.RevocationReason
        };
    }

    public static CertificateSimpleResponse ToSimpleResponse(this Certificate entity)
    {
        return new CertificateSimpleResponse
        {
            Id = entity.Id,
            CertificateNumber = entity.CertificateNumber,
            CourseTitle = entity.CourseTitle,
            IssuedDate = entity.IssuedDate,
            VerificationHash = entity.VerificationHash
        };
    }

    public static CertificateDetailResponse ToDetailResponse(this Certificate entity)
    {
        return new CertificateDetailResponse
        {
            Id = entity.Id,
            EnrollmentId = entity.EnrollmentId,
            CourseId = entity.CourseId,
            CertificateNumber = entity.CertificateNumber,
            StudentName = entity.StudentName,
            CourseTitle = entity.CourseTitle,
            InstructorName = entity.InstructorName,
            CompletionDate = entity.CompletionDate,
            IssuedDate = entity.IssuedDate,
            CertificatePdfUrl = entity.CertificatePdfUrl,
            CertificateImageUrl = entity.CertificateImageUrl,
            VerificationHash = entity.VerificationHash,
            IsValid = entity.IsValid,
            RevokedAt = entity.RevokedAt,
            RevocationReason = entity.RevocationReason
        };
    }

    public static Certificate ToCertificateEntity(
        this Enrollment enrollment,
        DateTime issuedAt,
        string studentName = "Học viên")
    {
        var completionDate = enrollment.CompletedAt ?? issuedAt;
        return new Certificate
        {
            EnrollmentId = enrollment.Id,
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            CertificateNumber = "CERT-" + Guid.NewGuid().ToString("N")[..16],
            StudentName = studentName,
            CourseTitle = enrollment.CourseTitle,
            InstructorName = enrollment.InstructorName,
            CompletionDate = completionDate,
            IssuedDate = issuedAt,
            VerificationHash = Guid.NewGuid().ToString("N"),
            IsValid = true
        };
    }
}
