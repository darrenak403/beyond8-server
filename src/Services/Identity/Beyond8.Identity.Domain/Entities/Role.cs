using System.ComponentModel.DataAnnotations;
using Beyond8.Common.Data.Base;

namespace Beyond8.Identity.Domain.Entities;

public class Role : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = null!; // ROLE_STUDENT, ROLE_INSTRUCTOR, ROLE_STAFF, ROLE_ADMIN
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!; // Student, Instructor, Staff, Admin
    [MaxLength(1000)]
    public string? Description { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
