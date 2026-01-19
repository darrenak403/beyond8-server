using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for course content generation
/// Examples: Generate lesson outlines, explain concepts, create examples, practice exercises
/// </summary>
public static class CourseContentPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            // TODO: Add your course content prompts here
            // Example template structure:
            /*
            new AiPrompt
            {
                Name = "Explain Complex Concept",
                Description = "Break down complex concepts into simple explanations",
                Category = PromptCategory.CourseContent,
                Template = @"Your prompt template with {variables} here",
                SystemPrompt = "System instructions for AI behavior",
                Version = "1.0",
                IsActive = true,
                Variables = @"{""variable1"": ""description"", ""variable2"": ""description""}",
                MaxTokens = 2000,
                Temperature = 0.7m,
                TopP = 0.9m,
                Tags = "explanation,concept,learning"
            }
            */
        };
    }
}
