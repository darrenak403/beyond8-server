using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.JSONFields;
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
            AttachmentUrls = DeserializeAttachmentList(entity.AttachmentUrls),
            SubmissionType = entity.SubmissionType,
            AllowedFileTypes = DeserializeStringList(entity.AllowedFileTypes),
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
            AttachmentUrls = SerializeAttachmentList(request.AttachmentUrls),
            SubmissionType = request.SubmissionType,
            AllowedFileTypes = SerializeStringList(request.AllowedFileTypes),
            MaxTextLength = request.MaxTextLength,
            GradingMode = request.GradingMode,
            TotalPoints = request.TotalPoints,
            RubricUrl = request.RubricUrl,
            TimeLimitMinutes = request.TimeLimitMinutes
        };
    }

    public static void UpdateFromRequest(this Assignment assignment, UpdateAssignmentRequest request)
    {
        assignment.CourseId = request.CourseId;
        assignment.SectionId = request.SectionId;
        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.AttachmentUrls = SerializeAttachmentList(request.AttachmentUrls);
        assignment.SubmissionType = request.SubmissionType;
        assignment.AllowedFileTypes = SerializeStringList(request.AllowedFileTypes);
        assignment.MaxTextLength = request.MaxTextLength;
        assignment.GradingMode = request.GradingMode;
        assignment.TotalPoints = request.TotalPoints;
        assignment.RubricUrl = request.RubricUrl;
        assignment.TimeLimitMinutes = request.TimeLimitMinutes;
    }

    private static string? SerializeAttachmentList(List<AssignmentAttachmentItem>? list)
    {
        if (list == null || list.Count == 0)
            return null;
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static List<AssignmentAttachmentItem>? DeserializeAttachmentList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<List<AssignmentAttachmentItem>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeStringList(List<string>? list)
    {
        if (list == null || list.Count == 0)
            return null;
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static List<string>? DeserializeStringList(string? json)
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
