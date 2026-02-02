namespace Beyond8.Assessment.Application.Dtos.Quizzes
{
    public class QuizSimpleResponse
    {
        public Guid Id { get; set; }
        public Guid InstructorId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int PassScorePercent { get; set; }
        public int QuestionCount { get; set; }
    }
}