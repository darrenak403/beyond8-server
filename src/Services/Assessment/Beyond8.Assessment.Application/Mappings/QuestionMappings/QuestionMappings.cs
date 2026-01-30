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
    }
}