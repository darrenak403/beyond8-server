namespace Beyond8.Common.Events.Catalog;

public record CourseUpdatedMetadataEvent(
    Guid CourseId,
    string Title,
    string? Price,
    string? ThumbnailUrl
);