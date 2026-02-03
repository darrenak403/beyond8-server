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

    public static void UpdateFromRequest(this AssignmentSubmission entity, GradeSubmissionRequest request, Guid gradedBy)
    {
        if (request.FinalScore > 0)
            entity.FinalScore = request.FinalScore;
        if (!string.IsNullOrEmpty(request.InstructorFeedback))
            entity.InstructorFeedback = request.InstructorFeedback;
        entity.Status = SubmissionStatus.Graded;
        entity.GradedAt = DateTime.UtcNow;
        entity.GradedBy = gradedBy;
    }

    public static SubmissionSimpleResponse ToSimpleResponse(this AssignmentSubmission entity)
    {
        return new SubmissionSimpleResponse
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            AssignmentId = entity.AssignmentId,
            SubmissionNumber = entity.SubmissionNumber,
            SubmittedAt = entity.SubmittedAt,
            AiScore = entity.AiScore,
            FinalScore = entity.FinalScore,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
        };
    }
}
