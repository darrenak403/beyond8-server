using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for content analysis
/// Examples: Summarize content, extract key concepts, analyze difficulty level
/// </summary>
public static class ContentAnalysisPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            // TODO: Add your content analysis prompts here
            // Examples:
            // - Summarize lecture notes
            // - Extract key learning objectives
            // - Analyze reading difficulty level
            // - Identify prerequisite knowledge
            // - Generate concept maps
        };
    }
}
