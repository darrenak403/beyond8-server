using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for assessment and quiz generation
/// Examples: Generate quiz questions, create rubrics, design assignments, test questions
/// </summary>
public static class AssessmentPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            new AiPrompt
            {
                Name = "Quiz Generation from Curriculum",
                Description = "Generate quiz questions based on provided curriculum content",
                Category = PromptCategory.Assessment,
                Template = @"Bạn là một giảng viên chuyên nghiệp. Hãy tạo {questionCount} câu hỏi trắc nghiệm dựa trên nội dung giáo trình sau:

Nội dung giáo trình:
{context}

Yêu cầu:
- Mỗi câu hỏi có 4 đáp án (A, B, C, D)
- Đánh dấu rõ đáp án đúng
- Câu hỏi phải bám sát nội dung
- Độ khó: {difficulty}
- Chủ đề: {topic}",
                SystemPrompt = "Bạn là trợ lý AI chuyên tạo câu hỏi đánh giá chất lượng cao cho giáo dục.",
                Version = "1.0",
                IsActive = true,
                Variables = @"{""questionCount"": ""số lượng câu hỏi"", ""context"": ""nội dung giáo trình"", ""difficulty"": ""Dễ/Trung bình/Khó"", ""topic"": ""chủ đề cụ thể""}",
                MaxTokens = 4000,
                Temperature = 0.7m,
                TopP = 0.9m,
                Tags = "quiz,assessment,education"
            },
            // TODO: Add more assessment prompts here
            // Examples:
            // - Create multiple choice questions
            // - Generate true/false questions
            // - Design essay prompts
            // - Create practical assignments
            // - Generate exam questions
        };
    }
}
