using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Users;

public class UpdateUserStatusRequest
{
    [Required(ErrorMessage = "Trạng thái tài khoản là bắt buộc.")]
    public UserStatus NewStatus { get; set; }
}
