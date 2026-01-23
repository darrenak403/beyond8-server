using System.Text.Json;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Mappings.AuthMappings;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Mappings;

public static class InstructorProfileMappings
{
    public static InstructorProfileResponse ToInstructorProfileResponse(this InstructorProfile instructorProfile, User user)
    {
        return new InstructorProfileResponse
        {
            Id = instructorProfile.Id,
            User = user.ToUserSimpleResponse(),
            Bio = instructorProfile.Bio,
            Headline = instructorProfile.Headline,
            ExpertiseAreas = string.IsNullOrEmpty(instructorProfile.ExpertiseAreas)
                ? null
                : JsonSerializer.Deserialize<List<string>>(instructorProfile.ExpertiseAreas),
            Education = string.IsNullOrEmpty(instructorProfile.Education)
                ? null
                : JsonSerializer.Deserialize<List<EducationInfo>>(instructorProfile.Education),
            WorkExperience = string.IsNullOrEmpty(instructorProfile.WorkExperience)
                ? null
                : JsonSerializer.Deserialize<List<WorkInfo>>(instructorProfile.WorkExperience),
            SocialLinks = string.IsNullOrEmpty(instructorProfile.SocialLinks)
                ? null
                : JsonSerializer.Deserialize<SocialInfo>(instructorProfile.SocialLinks),
            Certificates = string.IsNullOrEmpty(instructorProfile.Certificates)
                ? null
                : JsonSerializer.Deserialize<List<CertificateInfo>>(instructorProfile.Certificates),
            TeachingLanguages = instructorProfile.TeachingLanguages,
            IntroVideoUrl = instructorProfile.IntroVideoUrl,
            TotalStudents = instructorProfile.TotalStudents,
            TotalCourses = instructorProfile.TotalCourses,
            AvgRating = instructorProfile.AvgRating,
            VerificationStatus = instructorProfile.VerificationStatus,
            VerifiedAt = instructorProfile.VerifiedAt,
            CreatedAt = instructorProfile.CreatedAt,
            UpdatedAt = instructorProfile.UpdatedAt
        };
    }

    public static InstructorProfileAdminResponse ToInstructorProfileAdminResponse(this InstructorProfile instructorProfile, User user)
    {
        return new InstructorProfileAdminResponse
        {
            Id = instructorProfile.Id,
            User = user.ToUserSimpleResponse(),
            Bio = instructorProfile.Bio,
            Headline = instructorProfile.Headline,
            ExpertiseAreas = string.IsNullOrEmpty(instructorProfile.ExpertiseAreas)
                ? null
                : JsonSerializer.Deserialize<List<string>>(instructorProfile.ExpertiseAreas),
            Education = string.IsNullOrEmpty(instructorProfile.Education)
                ? null
                : JsonSerializer.Deserialize<List<EducationInfo>>(instructorProfile.Education),
            WorkExperience = string.IsNullOrEmpty(instructorProfile.WorkExperience)
                ? null
                : JsonSerializer.Deserialize<List<WorkInfo>>(instructorProfile.WorkExperience),
            SocialLinks = string.IsNullOrEmpty(instructorProfile.SocialLinks)
                ? null
                : JsonSerializer.Deserialize<SocialInfo>(instructorProfile.SocialLinks),
            TeachingLanguages = instructorProfile.TeachingLanguages,
            IntroVideoUrl = instructorProfile.IntroVideoUrl,
            TotalStudents = instructorProfile.TotalStudents,
            TotalCourses = instructorProfile.TotalCourses,
            AvgRating = instructorProfile.AvgRating,
            VerificationStatus = instructorProfile.VerificationStatus,
            VerifiedAt = instructorProfile.VerifiedAt,
            // Admin-only fields
            BankInfo = string.IsNullOrEmpty(instructorProfile.BankInfo)
                ? null
                : JsonSerializer.Deserialize<BankInfo>(instructorProfile.BankInfo),
            TaxId = instructorProfile.TaxId,
            IdentityDocuments = string.IsNullOrEmpty(instructorProfile.IdentityDocuments)
                ? null
                : JsonSerializer.Deserialize<List<IdentityInfo>>(instructorProfile.IdentityDocuments),
            VerificationNotes = instructorProfile.VerificationNotes,
            VerifiedBy = instructorProfile.VerifiedBy,
            CreatedAt = instructorProfile.CreatedAt,
            UpdatedAt = instructorProfile.UpdatedAt
        };
    }

    public static InstructorProfile ToInstructorProfileEntity(this CreateInstructorProfileRequest request, Guid userId)
    {
        return new InstructorProfile
        {
            UserId = userId,
            Bio = request.Bio,
            Headline = request.Headline,
            ExpertiseAreas = request.ExpertiseAreas.Any()
                ? JsonSerializer.Serialize(request.ExpertiseAreas)
                : null,
            Education = request.Education.Any()
                ? JsonSerializer.Serialize(request.Education)
                : null,
            WorkExperience = request.WorkExperience?.Any() == true
                ? JsonSerializer.Serialize(request.WorkExperience)
                : null,
            SocialLinks = request.SocialLinks != null
                ? JsonSerializer.Serialize(request.SocialLinks)
                : null,
            BankInfo = JsonSerializer.Serialize(request.BankInfo),
            TaxId = request.TaxId,
            TeachingLanguages = request.TeachingLanguages,
            IntroVideoUrl = request.IntroVideoUrl,
            IdentityDocuments = request.IdentityDocuments.Any()
                ? JsonSerializer.Serialize(request.IdentityDocuments)
                : null,
            Certificates = request.Certificates?.Any() == true
                ? JsonSerializer.Serialize(request.Certificates)
                : null,
            VerificationStatus = VerificationStatus.Pending
        };
    }

    public static void ToUpdateInstructorProfileRequest(this InstructorProfile instructorProfile, UpdateInstructorProfileRequest request)
    {
        if (request.Bio != null)
            instructorProfile.Bio = request.Bio;

        if (request.Headline != null)
            instructorProfile.Headline = request.Headline;

        if (request.ExpertiseAreas != null)
            instructorProfile.ExpertiseAreas = request.ExpertiseAreas.Any()
                ? JsonSerializer.Serialize(request.ExpertiseAreas)
                : null;

        if (request.Education != null)
            instructorProfile.Education = request.Education.Any()
                ? JsonSerializer.Serialize(request.Education)
                : null;

        if (request.WorkExperience != null)
            instructorProfile.WorkExperience = request.WorkExperience.Any()
                ? JsonSerializer.Serialize(request.WorkExperience)
                : null;

        if (request.SocialLinks != null)
            instructorProfile.SocialLinks = JsonSerializer.Serialize(request.SocialLinks);

        if (request.BankInfo != null)
            instructorProfile.BankInfo = JsonSerializer.Serialize(request.BankInfo);

        if (request.TaxId != null)
            instructorProfile.TaxId = request.TaxId;

        if (request.TeachingLanguages != null)
            instructorProfile.TeachingLanguages = request.TeachingLanguages;

        if (request.IntroVideoUrl != null)
            instructorProfile.IntroVideoUrl = request.IntroVideoUrl;

        if (request.IdentityDocuments != null)
            instructorProfile.IdentityDocuments = request.IdentityDocuments.Any()
                ? JsonSerializer.Serialize(request.IdentityDocuments)
                : null;

        if (request.Certificates != null)
            instructorProfile.Certificates = request.Certificates.Any()
                ? JsonSerializer.Serialize(request.Certificates)
                : null;

        instructorProfile.VerificationStatus = VerificationStatus.RequestUpdate;
    }
}