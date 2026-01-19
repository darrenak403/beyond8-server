using System.ComponentModel.DataAnnotations;

public class RejectInstructorApplicationRequest
{
    [Required(ErrorMessage = "Lý do từ chối không được để trống")]
    [MinLength(20, ErrorMessage = "Lý do từ chối phải có ít nhất 20 ký tự")]
    [MaxLength(500, ErrorMessage = "Lý do từ chối không được vượt quá 500 ký tự")]
    public string RejectionReason { get; set; } = string.Empty;
}