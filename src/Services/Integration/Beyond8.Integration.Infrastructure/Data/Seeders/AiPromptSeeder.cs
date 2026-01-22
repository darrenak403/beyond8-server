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
        return new List<AiPrompt>();
    }

    private static List<AiPrompt> GetFeedbackPrompts()
    {
        // TODO: Add feedback prompts
        // Examples: Provide constructive feedback, grade with rubric, suggest improvements
        return new List<AiPrompt>();
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
                Description = "Đánh giá hồ sơ ứng tuyển giảng viên theo tiêu chí Valid/Warning/Invalid. Trả về JSON.",
                Category = PromptCategory.Moderation,
                Template = @"Bạn là hệ thống thuật toán chấm điểm hồ sơ giảng viên (Profile Grader) hoạt động theo cơ chế TẤT ĐỊNH (Deterministic).

Nhiệm vụ: Phân tích dữ liệu đầu vào và trả về kết quả JSON duy nhất. KHÔNG trả về markdown, KHÔNG giải thích thêm.

QUY TẮC CHẤM ĐIỂM (RUBRIC & TRỌNG SỐ):
Để đảm bảo kết quả nhất quán 100%, bạn PHẢI chấm điểm dựa trên các tiêu chí sau, không được dùng cảm tính:

1. Bio & Headline (Trọng số: 10%)
- [90-100]: Có Headline chuyên nghiệp + Bio > 50 từ, mô tả rõ phương pháp/tầm nhìn giảng dạy.
- [50-89]: Có thông tin nhưng sơ sài, Bio ngắn (< 50 từ) hoặc viết chung chung.
- [0-49]: Bỏ trống, quá ngắn hoặc nội dung vô nghĩa/spam.

2. Expertise Areas (Trọng số: 15%)
- [90-100]: Liệt kê > 3 kỹ năng chuyên môn cụ thể, có phân cấp chính/phụ.
- [50-89]: Liệt kê 1-3 kỹ năng hoặc chỉ ghi tên lĩnh vực chung (ví dụ: ""IT"", ""Marketing"").
- [0-49]: Không có dữ liệu.

3. Education (Trọng số: 20%)
- [90-100]: Bằng Đại học/Cao đẳng trở lên + Ghi rõ Tên trường, Chuyên ngành và Năm tốt nghiệp.
- [50-89]: Có tên trường nhưng thiếu chuyên ngành hoặc thiếu năm tháng.
- [0-49]: Không có bằng cấp hoặc bằng cấp không liên quan đến giảng dạy.

4. Work Experience (Trọng số: 35% - QUAN TRỌNG NHẤT)
- [90-100]: > 2 năm kinh nghiệm + Mô tả chi tiết nhiệm vụ (bullet points) + Timeline logic, liên tục.
- [50-89]: Có liệt kê nơi làm việc nhưng mô tả sơ sài, hoặc timeline bị đứt quãng/phi lý.
- [0-49]: < 1 năm kinh nghiệm, hoặc chỉ ghi tên công ty mà không có mô tả.

5. Certificates (Trọng số: 20%)
- [90-100]: Có tên chứng chỉ uy tín + Tổ chức cấp + Ngày cấp (Nếu có ảnh đính kèm: Ảnh rõ nét, khớp text).
- [50-89]: Có tên chứng chỉ nhưng thiếu thông tin tổ chức/ngày tháng (Nếu có ảnh: Ảnh mờ/cắt góc).
- [0-49]: Không có chứng chỉ.

CÔNG THỨC TÍNH & STATUS:
1. totalScore = (Bio*0.1) + (Expertise*0.15) + (Education*0.2) + (WorkExperience*0.35) + (Certificates*0.2). Làm tròn về số nguyên gần nhất.
2. Status quy đổi từ Score của từng phần:
   - Score >= 80: ""Valid""
   - 50 <= Score < 80: ""Warning""
   - Score < 50: ""Invalid""
3. isAccepted = true NẾU totalScore >= 50.

OUTPUT FORMAT (JSON Schema):
{
  ""isAccepted"": boolean,
  ""totalScore"": number,
  ""feedbackSummary"": ""Tóm tắt ngắn gọn < 30 từ, giọng văn khách quan, tiếng Việt"",
  ""details"": [
    {
      ""sectionName"": ""Bio & Headline"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [""Liệt kê vấn đề ngắn gọn""],
      ""suggestions"": [""Hành động cụ thể để sửa""]
    },
    {
      ""sectionName"": ""Expertise Areas"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [],
      ""suggestions"": []
    },
    {
      ""sectionName"": ""Education"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [],
      ""suggestions"": []
    },
    {
      ""sectionName"": ""Work Experience"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [],
      ""suggestions"": []
    },
    {
      ""sectionName"": ""Certificates"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [],
      ""suggestions"": []
    }
  ],
  ""additionalFeedback"": ""Lời khuyên tổng thể tiếng Việt""
}

--- HỒ SƠ ĐẦU VÀO ---
{ApplicationText}",
                SystemPrompt = "Bạn là thuật toán kiểm duyệt (Moderation Algorithm) nghiêm ngặt. Nhiệm vụ của bạn là so khớp dữ liệu với Rubric đã định nghĩa để chấm điểm chính xác, không đưa ra ý kiến cá nhân.",
                Version = "1.0.1",
                IsActive = true,
                Variables = @"{""ApplicationText"": ""JSON string của ProfileReviewRequest""}",
                MaxTokens = 4096,
                Temperature = 0m,
                TopP = 1.0m,
                Tags = "moderation,instructor,application-review"
            }
        };
    }
}
