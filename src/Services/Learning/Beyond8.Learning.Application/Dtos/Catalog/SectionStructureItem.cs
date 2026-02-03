namespace Beyond8.Learning.Application.Dtos.Catalog;

public class SectionStructureItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<LessonStructureItem> Lessons { get; set; } = [];
}
