using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.Instructor;

public class CreateInstructorProfileRequest
{
    [Required(ErrorMessage = "Tiểu sử không được để trống")]
    [MinLength(50, ErrorMessage = "Tiểu sử phải có ít nhất 50 ký tự để thể hiện kinh nghiệm")]
    [MaxLength(300, ErrorMessage = "Tiểu sử không được vượt quá 300 ký tự")]
    public string Bio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề giới thiệu không được để trống")]
    [MinLength(10, ErrorMessage = "Tiêu đề phải có ít nhất 10 ký tự")]
    [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Headline { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lĩnh vực chuyên môn không được để trống")]
    [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 lĩnh vực chuyên môn")]
    [MaxLength(5, ErrorMessage = "Chỉ được chọn tối đa 5 lĩnh vực chuyên môn")]
    public List<string> ExpertiseAreas { get; set; } = new();

    [Required(ErrorMessage = "Thông tin học vấn không được để trống")]
    [MinLength(1, ErrorMessage = "Vui lòng cung cấp ít nhất 1 thông tin học vấn")]
    public List<EducationInfo> Education { get; set; } = new();

    public List<WorkInfo>? WorkExperience { get; set; }

    public SocialInfo? SocialLinks { get; set; }

    [Required(ErrorMessage = "Giấy tờ xác thực danh tính không được để trống")]
    [MinLength(1, ErrorMessage = "Vui lòng cung cấp ít nhất 1 giấy tờ xác thực")]
    public List<IdentityInfo> IdentityDocuments { get; set; } = new();

    public List<CertificateInfo>? Certificates { get; set; }
}
