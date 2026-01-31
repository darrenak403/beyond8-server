using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;

namespace Beyond8.Assessment.Application.Mappings.QuestionMappings
{
    public static class QuestionMappings
    {
        public static Question ToEntity(this QuestionRequest request, Guid instructorId, QuestionType type)
        {
            return new Question
            {
                InstructorId = instructorId,
                Content = request.Content,
                Type = type,
                Options = JsonSerializer.Serialize(request.Options),
                Explanation = request.Explanation,
                Tags = JsonSerializer.Serialize(request.Tags),
                Difficulty = request.Difficulty,
                Points = request.Points,
                IsActive = true,
            };
        }

        public static QuestionResponse ToResponse(this Question question)
        {
            return new QuestionResponse
            {
                Id = question.Id,
                Content = question.Content,
                Type = question.Type,
                Options = JsonSerializer.Deserialize<List<QuestionOptionItem>>(question.Options) ?? [],
                Explanation = question.Explanation,
                Tags = JsonSerializer.Deserialize<List<string>>(question.Tags) ?? [],
                Difficulty = question.Difficulty,
                Points = question.Points,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt ?? DateTime.UtcNow,
            };
        }

        public static void UpdateFromRequest(this Question question, QuestionRequest request)
        {
            if (!string.IsNullOrEmpty(request.Content)) question.Content = request.Content;
            question.Type = request.Type;
            question.Options = JsonSerializer.Serialize(request.Options);
            question.Explanation = request.Explanation;
            question.Tags = JsonSerializer.Serialize(request.Tags);
            question.Difficulty = request.Difficulty;
            question.Points = request.Points;
        }
    }
}