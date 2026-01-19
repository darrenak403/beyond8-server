using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for personalized learning
/// Examples: Adapt content to learning style, suggest learning paths, customize difficulty
/// </summary>
public static class PersonalizationPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            // TODO: Add your personalization prompts here
            // Examples:
            // - Adapt content for visual/auditory/kinesthetic learners
            // - Suggest personalized learning paths
            // - Adjust difficulty based on performance
            // - Recommend related topics
            // - Generate personalized practice problems
        };
    }
}
