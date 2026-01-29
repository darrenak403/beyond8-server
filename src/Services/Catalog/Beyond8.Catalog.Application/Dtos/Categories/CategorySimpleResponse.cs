using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Categories
{
    public class CategorySimpleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public CategoryType Type { get; set; } = CategoryType.Other;
        public bool IsRoot { get; set; } = false;
    }
}
