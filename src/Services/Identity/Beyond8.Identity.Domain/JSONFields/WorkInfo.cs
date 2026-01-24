namespace Beyond8.Identity.Domain.JSONFields;

public class WorkInfo
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime From { get; set; } = DateTime.MinValue;
    public DateTime? To { get; set; } = null;
    public bool IsCurrentJob { get; set; } = false;
    public string? Description { get; set; }
}