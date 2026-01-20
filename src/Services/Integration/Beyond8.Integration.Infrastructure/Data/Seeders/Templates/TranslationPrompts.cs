using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for translation and localization
/// Examples: Translate educational content, localize terminology, adapt cultural context
/// </summary>
public static class TranslationPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            // TODO: Add your translation prompts here
            // Examples:
            // - Translate course materials
            // - Localize educational terminology
            // - Adapt content for cultural context
            // - Translate quiz questions
            // - Convert technical documentation
        };
    }
}
