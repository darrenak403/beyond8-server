namespace Beyond8.Catalog.Application.Dtos.Categories;

public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Level { get; set; }

    public List<CategoryTreeDto> SubCategories { get; set; } = new();
}

