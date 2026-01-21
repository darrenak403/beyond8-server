using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Data.Seeders;

public static class AiPromptSeeder
{
    public static async Task SeedAsync(IntegrationDbContext context)
    {
        if (await context.AiPrompts.AnyAsync())
        {
            return; // Already seeded
        }

        var prompts = new List<AiPrompt>();

        // Add prompts from each category
        prompts.AddRange(GetCourseContentPrompts());
        prompts.AddRange(GetAssessmentPrompts());
        prompts.AddRange(GetFeedbackPrompts());
        prompts.AddRange(GetContentAnalysisPrompts());
        prompts.AddRange(GetTranslationPrompts());
        prompts.AddRange(GetPersonalizationPrompts());
        prompts.AddRange(GetModerationPrompts());

        await context.AiPrompts.AddRangeAsync(prompts);
        await context.SaveChangesAsync();
    }

    private static List<AiPrompt> GetCourseContentPrompts()
    {
        // TODO: Add course content prompts
        // Examples: Generate lesson outlines, explain concepts, create examples
        return new List<AiPrompt>();
    }

    private static List<AiPrompt> GetAssessmentPrompts()
    {
        // TODO: Add assessment prompts
        // Examples: Generate quiz questions, create rubrics, design assignments
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
            }
        };
    }

    private static List<AiPrompt> GetFeedbackPrompts()
    {
        // TODO: Add feedback prompts
        // Examples: Provide constructive feedback, grade with rubric, suggest improvements
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
            }
        };
    }

    private static List<AiPrompt> GetContentAnalysisPrompts()
    {
        // TODO: Add content analysis prompts
        // Examples: Summarize content, extract key concepts, analyze difficulty
        return new List<AiPrompt>();
    }

    private static List<AiPrompt> GetTranslationPrompts()
    {
        // TODO: Add translation prompts
        // Examples: Translate educational content, localize terminology
        return new List<AiPrompt>();
    }

    private static List<AiPrompt> GetPersonalizationPrompts()
    {
        // TODO: Add personalization prompts
        // Examples: Adapt content to learning style, suggest learning path
        return new List<AiPrompt>();
    }

    private static List<AiPrompt> GetModerationPrompts()
    {
        return new List<AiPrompt>
        {
            new AiPrompt
            {
                Name = "Instructor Application Review",
                Description = "Đánh giá hồ sơ ứng tuyển giảng viên. Trả về JSON.",
                Category = PromptCategory.Moderation,
                Template = @"Bạn là chuyên gia đánh giá hồ sơ giảng viên.

Nhiệm vụ: Đánh giá hồ sơ theo 6 phần:
- Bio & Headline
- Expertise Areas
- Education
- Work Experience
- Identity Documents
- Certificates

Với mỗi phần, trả về:
- status: ""Valid"" | ""Warning"" | ""Invalid""
- score: 0–100
- issues: liệt kê ngắn gọn vấn đề chính
- suggestions: gợi ý cải thiện, đi thẳng vào hành động

Riêng Identity Documents và Certificates: đánh giá cả nội dung text và ảnh đính kèm.

Quy tắc:
- totalScore = trung bình có trọng số các phần
- isAccepted = true chỉ khi totalScore ≥ 70 và KHÔNG có phần nào status = ""Invalid""

Chỉ trả về MỘT JSON object (không markdown, không text thừa):

{
  ""isAccepted"": boolean,
  ""totalScore"": number,
  ""feedbackSummary"": ""tóm tắt ngắn gọn, thân thiện, tiếng Việt"",
  ""details"": [
    {
      ""sectionName"": string,
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [string],
      ""suggestions"": [string]
    }
  ],
  ""additionalFeedback"": ""lời khuyên tổng thể, tiếng Việt""
}

--- HỒ SƠ ---
{ApplicationText}",
                SystemPrompt = "Bạn là chuyên gia tuyển dụng giảng viên, đánh giá công bằng, khách quan.",
                Version = "1.0.0",
                IsActive = true,
                Variables = @"{""ApplicationText"": ""nội dung hồ sơ""}",
                MaxTokens = 4096,
                Temperature = 0.6m,
                TopP = 0.9m,
                Tags = "moderation,instructor,application-review"
            }
        };
    }
}
