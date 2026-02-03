using System;
using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Mappings.AssignmentSubmissionMappings;

public static class AssignmentSubmissionMappings
{
    public static AssignmentSubmission ToEntity(this CreateSubmissionRequest request, Guid assignmentId, Guid userId, int submissionNumber)
    {
        return new AssignmentSubmission
        {
            AssignmentId = assignmentId,
            StudentId = userId,
            SubmissionNumber = submissionNumber,
            TextContent = request.TextContent,
            FileUrls = request.FileUrls,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
        };
    }

    public static SubmissionResponse ToResponse(this AssignmentSubmission entity)
    {
        return new SubmissionResponse
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            AssignmentId = entity.AssignmentId,
            SubmissionNumber = entity.SubmissionNumber,
            SubmittedAt = entity.SubmittedAt,
            TextContent = entity.TextContent,
            FileUrls = entity.FileUrls,
            AiScore = entity.AiScore,
            AiFeedback = entity.AiFeedback,
            FinalScore = entity.FinalScore,
            InstructorFeedback = entity.InstructorFeedback,
            GradedBy = entity.GradedBy,
            GradedAt = entity.GradedAt,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt,
        };
    }
}
