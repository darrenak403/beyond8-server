using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class UpdateCourseContentRequest
{
    [Required(ErrorMessage = "Mô tả khóa học không được để trống")]
    public string Description { get; set; } = string.Empty;

    // Future: Add sections, lessons, etc.
    // public List<SectionDto> Sections { get; set; } = [];
}