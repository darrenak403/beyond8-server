namespace Beyond8.Common.Events.Catalog;

public record CourseUpdatedMetadataEvent(
    Guid CourseId,
    string Title,
    string Slug,
    string Price, // OriginalPrice
    string FinalPrice, // Price after discount
    string ThumbnailUrl
);