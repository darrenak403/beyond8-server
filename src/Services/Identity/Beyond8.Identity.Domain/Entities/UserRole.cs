using System.ComponentModel.DataAnnotations.Schema;

namespace Beyond8.Identity.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!; public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
