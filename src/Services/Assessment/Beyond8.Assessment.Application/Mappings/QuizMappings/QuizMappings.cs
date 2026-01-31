using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Domain.Entities;

namespace Beyond8.Assessment.Application.Mappings.QuizMappings;

public static class QuizMappings
{
    public static QuizResponse ToResponse(this Quiz quiz, int questionCount = 0)
    {
        return new QuizResponse
        {
            Id = quiz.Id,
            InstructorId = quiz.InstructorId,
            CourseId = quiz.CourseId,
            LessonId = quiz.LessonId,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassScorePercent = quiz.PassScorePercent,
            TotalPoints = quiz.TotalPoints,
            MaxAttempts = quiz.MaxAttempts,
            ShuffleQuestions = quiz.ShuffleQuestions,
            AllowReview = quiz.AllowReview,
            ShowExplanation = quiz.ShowExplanation,
            QuestionCount = questionCount,
            CreatedAt = quiz.CreatedAt,
            UpdatedAt = quiz.UpdatedAt ?? quiz.CreatedAt
        };
    }

    public static Quiz ToEntity(this CreateQuizRequest request, Guid instructorId)
    {
        return new Quiz
        {
            InstructorId = instructorId,
            CourseId = request.CourseId,
            LessonId = request.LessonId,
            Title = request.Title,
            Description = request.Description,
            TimeLimitMinutes = request.TimeLimitMinutes,
            PassScorePercent = request.PassScorePercent,
            TotalPoints = request.TotalPoints,
            MaxAttempts = request.MaxAttempts,
            ShuffleQuestions = request.ShuffleQuestions,
            AllowReview = request.AllowReview,
            ShowExplanation = request.ShowExplanation
        };
    }
}
