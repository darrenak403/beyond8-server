using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Categories
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; }
        public string? Path { get; set; }

        public bool IsActive { get; set; }
        public int TotalCourses { get; set; }

        public CategoryType Type { get; set; } = CategoryType.Other;
        public bool IsRoot { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
