using System.ComponentModel.DataAnnotations;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class UpdateCourseMetadataRequest
{
    [Required(ErrorMessage = "Tiêu đề khóa học không được để trống")]
    [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô tả ngắn không được vượt quá 1000 ký tự")]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Danh mục không được để trống")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Cấp độ khóa học không được để trống")]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [MaxLength(10, ErrorMessage = "Mã ngôn ngữ không được vượt quá 10 ký tự")]
    public string Language { get; set; } = "vi-VN";

    [Range(0, 100000000, ErrorMessage = "Giá khóa học phải từ 0 đến 100 triệu VND")]
    public decimal Price { get; set; } = 0;

    [Required(ErrorMessage = "URL thumbnail không được để trống")]
    [Url(ErrorMessage = "URL thumbnail không hợp lệ")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    public List<string> Outcomes { get; set; } = [];
    public List<string>? Requirements { get; set; }
    public List<string>? TargetAudience { get; set; }
}