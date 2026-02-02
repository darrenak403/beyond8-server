namespace Beyond8.Common.Events.Catalog
{
    public record TranscodingVideoSuccessEvent
    (
        Guid InstructorId,
        Guid LessonId,
        string LessonTitle
    );
}