using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities
{
    public class Category : BaseEntity
    {
        public Guid? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = [];

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Level { get; set; } = 0;

        [MaxLength(500)]
        public string? Path { get; set; }

        public bool IsActive { get; set; } = true;

        public int TotalCourses { get; set; } = 0;

        public bool IsRoot { get; set; } = false;

        public CategoryType Type { get; set; } = CategoryType.Other;

        public virtual ICollection<Course> Courses { get; set; } = [];
    }
}
