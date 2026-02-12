namespace Beyond8.Learning.Application.Dtos.Progress;

public class CurriculumProgressResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public decimal ProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public List<SectionProgressItem> Sections { get; set; } = [];
}

public class SectionProgressItem
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCompleted { get; set; }
    public bool AssignmentSubmitted { get; set; }
    public decimal? AssignmentGrade { get; set; }
    public DateTime? AssignmentSubmittedAt { get; set; }
    public DateTime? AssignmentGradedAt { get; set; }
    public List<LessonProgressItem> Lessons { get; set; } = [];
}

public class LessonProgressItem
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCompleted { get; set; }
}
