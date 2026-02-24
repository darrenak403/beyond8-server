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


    public static List<ReassignRequestItemDto> ToOverviewItems(
        this List<ReassignRequest> requests,
        IReadOnlyDictionary<Guid, string> quizTitles,
        IReadOnlyDictionary<Guid, string> assignmentTitles)
    {
        return requests.Select(r =>
        {
            var title = r.Type == ReassignType.Quiz
                ? quizTitles.GetValueOrDefault(r.SourceId, "")
                : assignmentTitles.GetValueOrDefault(r.SourceId, "");
            return r.ToOverviewItem(title);
        }).ToList();
    }

    public static ReassignHistory ToQuizResetHistory(
        Guid quizId,
        Guid studentId,
        Guid instructorId,
        Guid? lessonId,
        int deletedCount,
        DateTime resetAt)
    {
        return new ReassignHistory
        {
            Type = ReassignType.Quiz,
            SourceId = quizId,
            StudentId = studentId,
            ResetByInstructorId = instructorId,
            LessonId = lessonId,
            ResetAt = resetAt,
            DeletedCount = deletedCount,
            CreatedAt = resetAt,
            CreatedBy = instructorId
        };
    }

    public static ReassignHistory ToAssignmentResetHistory(
        Guid assignmentId,
        Guid studentId,
        Guid instructorId,
        Guid? sectionId,
        int deletedCount,
        DateTime resetAt)
    {
        return new ReassignHistory
        {
            Type = ReassignType.Assignment,
            SourceId = assignmentId,
            StudentId = studentId,
            ResetByInstructorId = instructorId,
            SectionId = sectionId,
            ResetAt = resetAt,
            DeletedCount = deletedCount,
            CreatedAt = resetAt,
            CreatedBy = instructorId
        };
    }

    internal static string? TruncateNote(string? note) => note == null ? null : note.Length > NoteMaxLength ? note[..NoteMaxLength] : note;
}
