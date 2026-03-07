namespace Beyond8.Learning.Application.Dtos.Catalog;

public class LessonStructureItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? DurationSeconds { get; set; }
}
