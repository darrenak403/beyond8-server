using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.Notifications;

/// <summary>
/// Response DTO for admin notification log.
/// Does NOT include Title and Message content for privacy.
/// </summary>
public class NotificationLogResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationTarget Target { get; set; }
    public NotificationStatus Status { get; set; }
    public List<NotificationChannel> Channels { get; set; } = [];
    public DateTime? ReadAt { get; set; }
    public bool? IsRead { get; set; }
    public NotificationContext Context { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
