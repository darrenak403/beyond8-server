using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class MoveLessonToSectionRequest
{
    [Required(ErrorMessage = "LessonId không được để trống")]
    public Guid LessonId { get; set; }

    [Required(ErrorMessage = "NewSectionId không được để trống")]
    public Guid NewSectionId { get; set; }

    [Required(ErrorMessage = "NewOrderIndex không được để trống")]
    [Range(1, int.MaxValue, ErrorMessage = "NewOrderIndex phải lớn hơn 0")]
    public int NewOrderIndex { get; set; }
}