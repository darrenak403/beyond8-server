using System.Text.Json;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Mappings;

public static class InstructorProfileMappings
{
    // public static InstructorProfileResponse ToResponse(this InstructorProfile profile)
    // {
    //     return new InstructorProfileResponse
    //     {
    //         Id = profile.Id,
    //         UserId = profile.UserId,
    //         FullName = profile.User?.FullName ?? string.Empty,
    //         Email = profile.User?.Email ?? string.Empty,
    //         AvatarUrl = profile.User?.AvatarUrl,
    //         Bio = profile.Bio,
    //         Headline = profile.Headline,
    //         ExpertiseAreas = DeserializeJson<List<string>>(profile.ExpertiseAreas),
    //         Education = DeserializeJson<List<EducationInfo>>(profile.Education),
    //         WorkExperience = DeserializeJson<List<WorkInfo>>(profile.WorkExperience),
    //         SocialLinks = DeserializeJson<SocialInfo>(profile.SocialLinks),
    //         Certificates = DeserializeJson<List<CertificateInfo>>(profile.Certificates),
    //         VerificationStatus = profile.VerificationStatus,
    //         VerificationNotes = profile.VerificationNotes,
    //         VerifiedAt = profile.VerifiedAt,
    //         TotalStudents = profile.TotalStudents,
    //         TotalCourses = profile.TotalCourses,
    //         AvgRating = profile.AvgRating,
    //         CreatedAt = profile.CreatedAt,
    //         UpdatedAt = profile.UpdatedAt
    //     };
    // }

    // /// <summary>
    // /// Map CreateInstructorProfileRequest sang InstructorProfile entity
    // /// </summary>
    // public static InstructorProfile ToEntity(this CreateInstructorProfileRequest request, Guid userId)
    // {
    //     return new InstructorProfile
    //     {
    //         UserId = userId,
    //         Bio = request.Bio,
    //         Headline = request.Headline,
    //         ExpertiseAreas = SerializeJson(request.ExpertiseAreas),
    //         Education = SerializeJson(request.Education),
    //         WorkExperience = SerializeJson(request.WorkExperience),
    //         SocialLinks = SerializeJson(request.SocialLinks),
    //         IdentityDocuments = SerializeJson(request.IdentityDocuments),
    //         Certificates = SerializeJson(request.Certificates),
    //         CreatedBy = userId
    //     };
    // }

    // /// <summary>
    // /// Update InstructorProfile entity từ UpdateInstructorProfileRequest
    // /// </summary>
    // public static void UpdateFromRequest(this InstructorProfile profile, UpdateInstructorProfileRequest request, Guid updatedBy)
    // {
    //     if (!string.IsNullOrWhiteSpace(request.Bio))
    //         profile.Bio = request.Bio;

    //     if (!string.IsNullOrWhiteSpace(request.Headline))
    //         profile.Headline = request.Headline;

    //     if (request.ExpertiseAreas != null && request.ExpertiseAreas.Any())
    //         profile.ExpertiseAreas = SerializeJson(request.ExpertiseAreas);

    //     if (request.Education != null && request.Education.Any())
    //         profile.Education = SerializeJson(request.Education);

    //     if (request.WorkExperience != null)
    //         profile.WorkExperience = SerializeJson(request.WorkExperience);

    //     if (request.SocialLinks != null)
    //         profile.SocialLinks = SerializeJson(request.SocialLinks);

    //     if (!string.IsNullOrWhiteSpace(request.BankInfo))
    //         profile.BankInfo = request.BankInfo;

    //     if (!string.IsNullOrWhiteSpace(request.TaxId))
    //         profile.TaxId = request.TaxId;

    //     if (request.Certificates != null)
    //         profile.Certificates = SerializeJson(request.Certificates);

    //     profile.UpdatedAt = DateTime.UtcNow;
    //     profile.UpdatedBy = updatedBy;
    // }

    // /// <summary>
    // /// Deserialize JSON string sang object
    // /// </summary>
    // private static T? DeserializeJson<T>(string? json)
    // {
    //     if (string.IsNullOrEmpty(json))
    //         return default;

    //     try
    //     {
    //         return JsonSerializer.Deserialize<T>(json);
    //     }
    //     catch
    //     {
    //         return default;
    //     }
    // }

    // /// <summary>
    // /// Serialize object sang JSON string
    // /// </summary>
    // private static string? SerializeJson<T>(T? obj)
    // {
    //     if (obj == null)
    //         return null;

    //     try
    //     {
    //         return JsonSerializer.Serialize(obj);
    //     }
    //     catch
    //     {
    //         return null;
    //     }
    // }
}