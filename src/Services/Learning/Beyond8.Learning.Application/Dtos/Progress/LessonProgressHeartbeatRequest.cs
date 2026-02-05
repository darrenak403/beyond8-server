namespace Beyond8.Learning.Application.Dtos.Progress;

public class LessonProgressHeartbeatRequest
{
    public int? LastPositionSeconds { get; set; }

    public bool MarkComplete { get; set; }
}
