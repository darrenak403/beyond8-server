using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for student feedback and grading
/// Examples: Review answers, provide constructive feedback, grade with rubric
/// </summary>
public static class FeedbackPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            new AiPrompt
            {
                Name = "Student Answer Review",
                Description = "Review and provide feedback on student answers",
                Category = PromptCategory.Feedback,
                Template = @"Bạn là giảng viên đang chấm bài. Hãy đánh giá câu trả lời của học sinh dựa trên tiêu chí sau:

Câu hỏi: {question}

Đáp án mẫu:
{modelAnswer}

Câu trả lời của học sinh:
{studentAnswer}

Hãy cung cấp:
1. Điểm số (trên thang điểm {maxScore})
2. Những điểm làm tốt
3. Những điểm cần cải thiện
4. Gợi ý để hoàn thiện câu trả lời",
                SystemPrompt = "Bạn là giảng viên giàu kinh nghiệm, cung cấp feedback mang tính xây dựng và khuyến khích học sinh.",
                Version = "1.0",
                IsActive = true,
                Variables = @"{""question"": ""câu hỏi"", ""modelAnswer"": ""đáp án mẫu"", ""studentAnswer"": ""câu trả lời của học sinh"", ""maxScore"": ""điểm tối đa""}",
                MaxTokens = 2000,
                Temperature = 0.6m,
                TopP = 0.9m,
                Tags = "feedback,review,grading"
            },
            new AiPrompt
            {
                Name = "Rubric-Based Grading",
                Description = "Grade student work based on detailed rubric criteria",
                Category = PromptCategory.Feedback,
                Template = @"Bạn là giảng viên chấm bài theo rubric. Hãy đánh giá bài làm của học sinh dựa trên rubric sau:

Rubric:
{rubric}

Bài làm của học sinh:
{submission}

Hãy:
1. Đánh giá từng tiêu chí trong rubric
2. Cho điểm từng tiêu chí
3. Tính tổng điểm
4. Đưa ra nhận xét tổng quan
5. Gợi ý cải thiện cụ thể",
                SystemPrompt = "Bạn là giảng viên chấm bài công bằng, khách quan, theo đúng rubric đã cho.",
                Version = "1.0",
                IsActive = true,
                Variables = @"{""rubric"": ""bảng tiêu chí đánh giá chi tiết"", ""submission"": ""bài làm của học sinh""}",
                MaxTokens = 3000,
                Temperature = 0.5m,
                TopP = 0.9m,
                Tags = "rubric,grading,assessment"
            },
            // TODO: Add more feedback prompts here
            // Examples:
            // - Provide motivational feedback
            // - Suggest improvement strategies
            // - Compare student progress over time
        };
    }
}
