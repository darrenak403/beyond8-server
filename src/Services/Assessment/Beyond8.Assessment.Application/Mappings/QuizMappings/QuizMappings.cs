using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Application.Mappings.QuestionMappings;
using Beyond8.Assessment.Domain.Entities;

namespace Beyond8.Assessment.Application.Mappings.QuizMappings;

public static class QuizMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static QuizResponse ToResponse(this Quiz quiz, List<Question> questions)
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
            DifficultyDistribution = DeserializeDifficultyDistribution(quiz.DifficultyDistribution),
            CreatedAt = quiz.CreatedAt,
            UpdatedAt = quiz.UpdatedAt ?? quiz.CreatedAt,
            Questions = [.. questions.Select(q => q.ToResponse())]
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
            ShowExplanation = request.ShowExplanation,
            DifficultyDistribution = request.DifficultyDistribution != null
                ? JsonSerializer.Serialize(request.DifficultyDistribution, JsonOptions)
                : null
        };
    }

    public static QuizSimpleResponse ToSimpleResponse(this Quiz quiz, int questionCount = 0)
    {
        return new QuizSimpleResponse
        {
            Id = quiz.Id,
            InstructorId = quiz.InstructorId,
            CourseId = quiz.CourseId,
            LessonId = quiz.LessonId,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassScorePercent = quiz.PassScorePercent,
            QuestionCount = questionCount,
        };
    }

    public static void UpdateFromRequest(this Quiz quiz, UpdateQuizRequest request)
    {
        if (!string.IsNullOrEmpty(request.Title)) quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        quiz.PassScorePercent = request.PassScorePercent;
        quiz.TotalPoints = request.TotalPoints;
        quiz.MaxAttempts = request.MaxAttempts;
        quiz.ShuffleQuestions = request.ShuffleQuestions;
        quiz.AllowReview = request.AllowReview;
        quiz.ShowExplanation = request.ShowExplanation;
        if (request.DifficultyDistribution != null)
            quiz.DifficultyDistribution = JsonSerializer.Serialize(request.DifficultyDistribution, JsonOptions);
    }

    private static DifficultyDistributionDto? DeserializeDifficultyDistribution(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<DifficultyDistributionDto>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
