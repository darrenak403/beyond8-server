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
                  Description = "Sinh câu hỏi trắc nghiệm (MCQ) chất lượng cao, phân loại theo thang đo Bloom; điểm số tính đúng theo tổng MaxPoints người dùng nhập.",
                  Category = PromptCategory.Assessment,
                  SystemPrompt = "Bạn là chuyên gia khảo thí. Nhiệm vụ: tạo câu hỏi trắc nghiệm khách quan từ context, tuân thủ cấu trúc JSON và quy tắc tính điểm bắt buộc (tổng điểm tất cả câu phải đúng bằng MaxPoints).",
                  Version = "3.0.0",
                  IsActive = true,
                  MaxTokens = 4096,
                  Temperature = 0.3m,
                  TopP = 0.9m,
                  Tags = "assessment,quiz,multiple-choice,bloom-taxonomy",
                  Template = @"INPUT:
---
CONTEXT (Nội dung khóa học):
{Context}
---
Chủ đề trọng tâm: {Query}

CẤU HÌNH (BẮT BUỘC TUÂN THỦ):
- Số câu: Easy = {EasyCount}, Medium = {MediumCount}, Hard = {HardCount}.
- Tổng điểm tối đa (MaxPoints) người dùng nhập: {MaxPoints}. Tổng điểm của tất cả câu hỏi trả về PHẢI đúng bằng {MaxPoints}.

TÍNH ĐIỂM TỪNG CÂU (QUAN TRỌNG – ĐỌC KỸ):
Trọng số theo độ khó: Easy : Medium : Hard = 1 : 1.5 : 2 (mỗi câu).
1) Tính: totalWeight = ({EasyCount} × 1) + ({MediumCount} × 1.5) + ({HardCount} × 2).
2) Đơn vị: X = {MaxPoints} / totalWeight.
3) Điểm mỗi câu: mỗi câu Easy = round(X, 1); mỗi câu Medium = round(1.5 × X, 1); mỗi câu Hard = round(2 × X, 1).
4) Kiểm tra: (số câu Easy × điểm Easy) + (số câu Medium × điểm Medium) + (số câu Hard × điểm Hard) = {MaxPoints}. Nếu lệch do làm tròn, điều chỉnh 1 câu (cộng/bớt 0.5 hoặc 1) để tổng đúng bằng {MaxPoints}. Trường ""points"" trong JSON phải là số (có thể thập phân, ví dụ 3.5).

TIÊU CHUẨN CÂU HỎI:
- Easy (nhận biết/thông hiểu): định nghĩa, khái niệm có trong context.
- Medium (vận dụng): tình huống/đoạn code ngắn, yêu cầu xác định kết quả hoặc lỗi.
- Hard (phân tích/đánh giá): so sánh giải pháp, nguyên nhân sâu, tối ưu hóa.
- Đáp án nhiễu: hợp lý, dễ gây nhầm. KHÔNG dùng ""Tất cả đáp án trên"" / ""Không đáp án nào đúng"".

OUTPUT:
- Chỉ trả về Raw JSON, KHÔNG markdown (```json), KHÔNG giải thích ngoài JSON.

Schema:
{{
  ""easy"": [
    {{ ""content"": ""..."", ""type"": 0, ""options"": [{{ ""id"": ""a"", ""text"": ""..."", ""isCorrect"": false }}, ...], ""explanation"": ""..."", ""tags"": [], ""difficulty"": 0, ""points"": <số điểm đã tính cho câu Easy> }}
  ],
  ""medium"": [ ... ],
  ""hard"": [ ... ]
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
