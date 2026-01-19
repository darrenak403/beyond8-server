using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.JSONFields;

public class UpdateInstructorProfileRequest
{
    [MinLength(50, ErrorMessage = "Tiểu sử phải có ít nhất 50 ký tự")]
    [MaxLength(300, ErrorMessage = "Tiểu sử không được vượt quá 300 ký tự")]
    public string? Bio { get; set; }

    [MinLength(10, ErrorMessage = "Tiêu đề phải có ít nhất 10 ký tự")]
    [MaxLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string? Headline { get; set; }

    [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 lĩnh vực chuyên môn")]
    [MaxLength(5, ErrorMessage = "Chỉ được chọn tối đa 5 lĩnh vực chuyên môn")]
    public List<string>? ExpertiseAreas { get; set; }

    public List<EducationInfo>? Education { get; set; }

    public List<WorkInfo>? WorkExperience { get; set; }

    public SocialInfo? SocialLinks { get; set; }

    [MaxLength(500, ErrorMessage = "Thông tin ngân hàng không được vượt quá 500 ký tự")]
    public string? BankInfo { get; set; }

    [MaxLength(50, ErrorMessage = "Mã số thuế không được vượt quá 50 ký tự")]
    public string? TaxId { get; set; }

    public List<CertificateInfo>? Certificates { get; set; }
}