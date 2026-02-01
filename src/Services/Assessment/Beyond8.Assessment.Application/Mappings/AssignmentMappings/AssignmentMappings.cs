using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Domain.Entities;

namespace Beyond8.Assessment.Application.Mappings.AssignmentMappings;

public static class AssignmentMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static AssignmentResponse ToResponse(this Assignment entity)
    {
        return new AssignmentResponse
        {
            Id = entity.Id,
            InstructorId = entity.InstructorId,
            CourseId = entity.CourseId,
            SectionId = entity.SectionId,
            Title = entity.Title,
            Description = entity.Description,
            AttachmentUrls = DeserializeList(entity.AttachmentUrls),
            SubmissionType = entity.SubmissionType,
            AllowedFileTypes = DeserializeList(entity.AllowedFileTypes),
            MaxTextLength = entity.MaxTextLength,
            GradingMode = entity.GradingMode,
            TotalPoints = entity.TotalPoints,
            RubricUrl = entity.RubricUrl,
            TimeLimitMinutes = entity.TimeLimitMinutes,
            TotalSubmissions = entity.TotalSubmissions,
            AverageScore = entity.AverageScore,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
        };
    }

    public static AssignmentSimpleResponse ToSimpleResponse(this Assignment entity)
    {
        return new AssignmentSimpleResponse
        {
            Id = entity.Id,
            InstructorId = entity.InstructorId,
            CourseId = entity.CourseId,
            SectionId = entity.SectionId,
            Title = entity.Title,
            TotalSubmissions = entity.TotalSubmissions,
            AverageScore = entity.AverageScore,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Assignment ToEntity(this CreateAssignmentRequest request, Guid instructorId)
    {
        return new Assignment
        {
            InstructorId = instructorId,
            CourseId = request.CourseId,
            SectionId = request.SectionId,
            Title = request.Title,
            Description = request.Description,
            AttachmentUrls = SerializeList(request.AttachmentUrls),
            SubmissionType = request.SubmissionType,
            AllowedFileTypes = SerializeList(request.AllowedFileTypes),
            MaxTextLength = request.MaxTextLength,
            GradingMode = request.GradingMode,
            TotalPoints = request.TotalPoints,
            RubricUrl = request.RubricUrl,
            TimeLimitMinutes = request.TimeLimitMinutes
        };
    }

    private static string? SerializeList(List<string>? list)
    {
        if (list == null || list.Count == 0)
            return null;
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static List<string>? DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
