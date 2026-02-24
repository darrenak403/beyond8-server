using Beyond8.Assessment.Application.Dtos.Reassign;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Mappings.ReassignMappings;

public static class ReassignMappings
{
    private const int NoteMaxLength = 500;

    public static ReassignRequest ToEntity(this RequestQuizReassignRequest request, Guid quizId, Guid studentId, DateTime now)
    {
        return new ReassignRequest
        {
            Type = ReassignType.Quiz,
            SourceId = quizId,
            StudentId = studentId,
            Reason = request.Reason,
            Note = TruncateNote(request.Note),
            Status = ReassignRequestStatus.Pending,
            RequestedAt = now,
            CreatedAt = now,
            CreatedBy = studentId
        };
    }

    public static ReassignRequest ToEntity(this RequestAssignmentReassignRequest request, Guid assignmentId, Guid studentId, DateTime now)
    {
        return new ReassignRequest
        {
            Type = ReassignType.Assignment,
            SourceId = assignmentId,
            StudentId = studentId,
            Reason = request.Reason,
            Note = TruncateNote(request.Note),
            Status = ReassignRequestStatus.Pending,
            RequestedAt = now,
            CreatedAt = now,
            CreatedBy = studentId
        };
    }

    public static ReassignRequestResponse ToResponse(this ReassignRequest entity, string message)
    {
        return new ReassignRequestResponse
        {
            Id = entity.Id,
            Status = entity.Status,
            Message = message
        };
    }

    public static ReassignRequestItemDto ToOverviewItem(this ReassignRequest entity, string sourceTitle)
    {
        return new ReassignRequestItemDto
        {
            Id = entity.Id,
            Type = entity.Type,
            SourceId = entity.SourceId,
            SourceTitle = sourceTitle,
            StudentId = entity.StudentId,
            Reason = entity.Reason,
            Note = entity.Note,
            RequestedAt = entity.RequestedAt,
            Status = entity.Status
        };
    }

    internal static string? TruncateNote(string? note) => note == null ? null : note.Length > NoteMaxLength ? note[..NoteMaxLength] : note;
}
