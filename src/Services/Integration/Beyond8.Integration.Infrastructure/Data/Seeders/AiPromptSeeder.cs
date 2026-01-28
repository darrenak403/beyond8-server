using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Data.Seeders
{
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
      return [];
    }

    private static List<AiPrompt> GetAssessmentPrompts()
    {
      return
      [
          new AiPrompt
              {
                  Name = "Quiz Generation",
                  Description = "Sinh câu hỏi trắc nghiệm (MCQ) chất lượng cao, phân loại theo thang đo Bloom và tự động tính trọng số điểm.",
                  Category = PromptCategory.Assessment,
                  SystemPrompt = "Bạn là một chuyên gia khảo thí và thiết kế chương trình giảng dạy. Nhiệm vụ của bạn là tạo ra các câu hỏi trắc nghiệm khách quan, chính xác về kiến thức, tuân thủ cấu trúc JSON nghiêm ngặt, và tính toán điểm số hợp lý.",
                  Version = "2.0.0",
                  IsActive = true,
                  MaxTokens = 4096,
                  Temperature = 0.3m,
                  TopP = 0.9m,
                  Tags = "assessment,quiz,multiple-choice,bloom-taxonomy",
                  Template = @"INPUT DATA:
---
CONTEXT (Nội dung khóa học):
{Context}
---
USER QUERY (Chủ đề trọng tâm): {Query}

CẤU HÌNH BÀI KIỂM TRA:
1. Số lượng: Easy ({EasyCount}), Medium ({MediumCount}), Hard ({HardCount}).
2. Tổng điểm tối đa (Total Max Points): {MaxPoints}.

TIÊU CHUẨN CHẤT LƯỢNG CÂU HỎI (QUAN TRỌNG):
- Độ khó Easy (Nhận biết/Thông hiểu): Hỏi về định nghĩa, khái niệm cơ bản có trong context.
- Độ khó Medium (Vận dụng): Đưa ra tình huống giả định hoặc đoạn code nhỏ, yêu cầu xác định kết quả hoặc lỗi sai.
- Độ khó Hard (Phân tích/Đánh giá): Yêu cầu so sánh các giải pháp, tìm nguyên nhân sâu xa của vấn đề phức tạp, hoặc tối ưu hóa.
- Đáp án nhiễu (Distractors): Phải có vẻ hợp lý (plausible), tránh các đáp án quá ngây ngô. KHÔNG dùng ""Tất cả đáp án trên"" hoặc ""Không đáp án nào đúng"".

CHIẾN LƯỢC PHÂN BỔ ĐIỂM SỐ:
Hãy tính toán điểm số (points) cho từng câu dựa trên độ khó theo tỷ lệ trọng số:
**Easy : Medium : Hard = 1 : 1.5 : 2**
(Quy trình: Tính giá trị đơn vị X sao cho tổng điểm = {MaxPoints}, sau đó gán điểm Easy=X, Medium=1.5X, Hard=2X. Làm tròn điểm đến 1 chữ số thập phân).

OUTPUT FORMAT:
- Chỉ trả về JSON thuần (Raw JSON).
- KHÔNG bọc trong markdown block (```json).
- KHÔNG thêm lời dẫn hay giải thích ngoài JSON.

JSON SCHEMA MẪU:
{{
  ""easy"": [
    {{
      ""content"": ""Câu hỏi mức độ nhớ/hiểu?"",
      ""type"": 0,
      ""options"": [
        {{ ""id"": ""a"", ""text"": ""Đáp án sai 1"", ""isCorrect"": false }},
        {{ ""id"": ""b"", ""text"": ""Đáp án ĐÚNG"", ""isCorrect"": true }},
        {{ ""id"": ""c"", ""text"": ""Đáp án sai 2"", ""isCorrect"": false }},
        {{ ""id"": ""d"", ""text"": ""Đáp án sai 3"", ""isCorrect"": false }}
      ],
      ""explanation"": ""Giải thích ngắn gọn tại sao đáp án đúng là đúng."",
      ""tags"": [""keyword""],
      ""difficulty"": 0,
      ""points"": 1.5
    }}
  ],
  ""medium"": [ ...tương tự... ],
  ""hard"": [ ...tương tự... ]
}}"
        }
      ];
    }

    private static List<AiPrompt> GetFeedbackPrompts()
    {
      // TODO: Add feedback prompts
      // Examples: Provide constructive feedback, grade with rubric, suggest improvements
      return [];
    }

    private static List<AiPrompt> GetContentAnalysisPrompts()
    {
      return [];
    }

    private static List<AiPrompt> GetTranslationPrompts()
    {
      return [];
    }

    private static List<AiPrompt> GetPersonalizationPrompts()
    {
      return [];
    }

    private static List<AiPrompt> GetModerationPrompts()
    {
      return
      [
          new AiPrompt
            {
                Name = "Instructor Profile Review",
                Description = "Đánh giá hồ sơ giảng viên với tiêu chí linh hoạt và giọng văn thân thiện, mang tính xây dựng. Trả về JSON.",
                Category = PromptCategory.Moderation,
                SystemPrompt = "Bạn là trợ lý AI thân thiện, chuyên nghiệp. Nhiệm vụ của bạn là đánh giá hồ sơ giảng viên với thái độ tích cực, mang tính xây dựng để giúp họ cải thiện, thay vì chỉ trích lỗi sai.",
                Version = "1.1.0",
                IsActive = true,
                Variables = @"{""ApplicationText"": ""JSON string của ProfileReviewRequest""}",
                MaxTokens = 4096,
                Temperature = 0.3m,
                TopP = 1.0m,
                Tags = "moderation,instructor,application-review,friendly",
                Template = @"Bạn là ""Người đồng hành phát triển hồ sơ giảng viên"". 
Nhiệm vụ: Phân tích dữ liệu và trả về JSON. KHÔNG trả về markdown, KHÔNG giải thích thêm ngoài JSON.

TONE & VOICE (QUAN TRỌNG):
- Ngôn ngữ: Tiếng Việt.
- Giọng văn: Thân thiện, khích lệ, mang tính xây dựng.
- Tuyệt đối tránh từ ngữ tiêu cực (như ""tệ"", ""sai"", ""vô nghĩa""). Hãy dùng cách diễn đạt nhẹ nhàng (như ""cần chi tiết hơn"", ""nên bổ sung"").
- Mục tiêu: Giúp người dùng cảm thấy muốn hoàn thiện hồ sơ chứ không phải bị phán xét.

QUY TẮC CHẤM ĐIỂM:

1. Bio & Headline (10%)
- [85-100]: Headline thu hút + Bio có tâm (đủ ý nghĩa).
- [50-84]: Có thông tin nhưng hơi ngắn hoặc chưa trau chuốt.
- [0-49]: Chưa điền hoặc quá sơ sài.

2. Expertise Areas (15%)
- [85-100]: Liệt kê kỹ năng rõ ràng, phù hợp giảng dạy.
- [50-84]: Có liệt kê nhưng còn chung chung.
- [0-49]: Chưa có dữ liệu.

3. Education (20%)
- [85-100]: Có bằng cấp/chứng chỉ hoặc quá trình học tập liên quan.
- [50-84]: Thông tin trường/ngành chưa chi tiết, hoặc trái ngành nhưng chấp nhận được (có kinh nghiệm bù lại).
- [0-49]: Chưa cập nhật.

4. Work Experience (35% - Trọng số cao)
- [85-100]: Có kinh nghiệm. Mô tả công việc dễ hiểu. Timeline logic.
- [50-84]: Có nơi làm việc nhưng mô tả chưa sâu. Chấp nhận Junior (< 1 năm) nếu viết tốt.
- [0-49]: Chỉ ghi tên công ty hoặc timeline chưa rõ.

5. Certificates (15%)
- [85-100]: Có tên chứng chỉ hỗ trợ chuyên môn.
- [50-84]: Có tên chứng chỉ nhưng thiếu nơi cấp/ngày tháng.
- [0-49]: Chưa có.

6. Teaching Languages (5%)
- [100]: Có chọn ngôn ngữ giảng dạy.
- [0]: Chưa chọn.

CÔNG THỨC & TRẠNG THÁI:
1. Tổng điểm = (Bio*0.1) + (Expertise*0.15) + (Education*0.2) + (WorkExperience*0.35) + (Certificates*0.15) + (TeachingLanguages*0.05).
2. Status:
   - Score >= 75: ""Valid""
   - 45 <= Score < 75: ""Warning""
   - Score < 45: ""Invalid""
3. isAccepted = true NẾU totalScore >= 45.

OUTPUT FORMAT (JSON Only):
{
  ""isAccepted"": boolean,
  ""totalScore"": number,
  ""feedbackSummary"": ""Tóm tắt < 30 từ, giọng văn khen ngợi điểm tốt trước, nhắc nhở điểm thiếu sau một cách nhẹ nhàng"",
  ""details"": [
    {
      ""sectionName"": ""Bio & Headline"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [""Diễn đạt vấn đề dưới dạng 'Cần cải thiện' thay vì 'Lỗi'""],
      ""suggestions"": [""Gợi ý hành động cụ thể bắt đầu bằng các động từ: Hãy, Nên, Thử...""]
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
    },
    {
      ""sectionName"": ""Teaching Languages"",
      ""status"": ""Valid"" | ""Warning"" | ""Invalid"",
      ""score"": number,
      ""issues"": [],
      ""suggestions"": []
    }
  ],
  ""additionalFeedback"": ""Lời khuyên tổng thể ấm áp, như một người bạn khuyên nhủ.""
}

--- HỒ SƠ ĐẦU VÀO ---
{ApplicationText}"
            }
      ];
    }
  }
}
