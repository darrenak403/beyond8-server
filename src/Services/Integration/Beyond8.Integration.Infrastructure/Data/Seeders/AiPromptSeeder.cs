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
            Description = "Sinh câu hỏi trắc nghiệm Bloom. Hỗ trợ điểm thập phân, tự động bù trừ sai số để tổng điểm chính xác tuyệt đối.",
            Category = PromptCategory.Assessment,
            SystemPrompt = "Bạn là chuyên gia Khảo thí và Toán học. Nhiệm vụ: Tạo JSON câu hỏi trắc nghiệm. Nguyên tắc vàng: Tổng điểm (Sum of Points) PHẢI bằng chính xác 'MaxPoints' yêu cầu.",
            Version = "3.3.0",
            IsActive = true,
            MaxTokens = 4096,
            Temperature = 0.2m,
            TopP = 0.9m,
            Tags = "assessment,quiz,bloom,calculation,json",
            Template = @"
INPUT:
---
CONTEXT: {Context}
---
CHỦ ĐỀ: {Query}

CẤU HÌNH YÊU CẦU:
- Số lượng: Easy={EasyCount}, Medium={MediumCount}, Hard={HardCount}.
- TỔNG ĐIỂM (MAX POINTS): {MaxPoints}

QUY TRÌNH TÍNH ĐIỂM (BẮT BUỘC TUÂN THỦ):
1. [Trọng số]: Easy=1, Medium=1.5, Hard=2.
2. [Tổng trọng số]: W_Total = ({EasyCount} * 1) + ({MediumCount} * 1.5) + ({HardCount} * 2).
3. [Giá trị đơn vị]: Unit = {MaxPoints} / W_Total.
4. [Phân bổ sơ bộ]:
   - Điểm mỗi câu Easy = Unit (giữ 2 số thập phân).
   - Điểm mỗi câu Medium = Unit * 1.5 (giữ 2 số thập phân).
   - Điểm mỗi câu Hard = Unit * 2 (giữ 2 số thập phân).
5. [BƯỚC BÙ TRỪ - QUAN TRỌNG NHẤT]:
   - Cộng tổng điểm sơ bộ của tất cả câu hỏi.
   - Tính sai số: Delta = {MaxPoints} - (Tổng sơ bộ).
   - Cộng giá trị Delta vào câu hỏi 'Hard' cuối cùng (hoặc câu cuối cùng của danh sách).
   => Kết quả: Tổng điểm cuối cùng phải bằng đúng {MaxPoints}.

YÊU CẦU NỘI DUNG:
- Format câu hỏi: Easy (Nhận biết), Medium (Vận dụng), Hard (Phân tích).
- Đáp án nhiễu (Distractors): Hợp lý, không lộ liễu. KHÔNG dùng ""Tất cả đáp án trên"".

OUTPUT FORMAT (JSON RAW):
{{
  ""easy"": [ 
    {{ ""content"": ""..."", ""type"": 0, ""options"": [{{ ""id"": ""a"", ""text"": ""..."", ""isCorrect"": false }}], ""explanation"": ""..."", ""difficulty"": 0, ""points"": <số thập phân> }} 
  ],
  ""medium"": [ ... ],
  ""hard"": [ 
    {{ ... ""difficulty"": 2, ""points"": <số thập phân đã cộng bù trừ> }} 
  ]
}}"
        },

        new AiPrompt
        {
            Name = "Format Quiz Questions",
            Description = "Trích xuất câu hỏi từ text PDF, chuẩn hóa điền từ thành 4 gạch dưới.",
            Category = PromptCategory.Assessment,
            SystemPrompt = "Bạn là chuyên gia xử lý dữ liệu văn bản. Nhiệm vụ: Trích xuất câu hỏi trắc nghiệm sang JSON và chuẩn hóa định dạng.",
            Version = "1.3.0",
            IsActive = true,
            MaxTokens = 4096,
            Temperature = 0.1m,
            TopP = 0.5m,
            Tags = "format,pdf,extract,json",
            Template = @"
INPUT TEXT (Nguồn PDF):
---
{Content}
---

NHIỆM VỤ CỤ THỂ:
1. [Lọc nhiễu]: Loại bỏ số trang, header/footer (ví dụ: 'Page 1', 'Chapter 3') nếu chúng xen vào giữa câu hỏi.
2. [Chuẩn hóa Content]: 
   - Tìm tất cả các ký hiệu biểu thị chỗ trống như: '...', '___', '..', '_____' hoặc khoảng trắng trong ngoặc (...).
   - Thay thế TẤT CẢ bằng duy nhất một định dạng: ____ (đúng 4 ký tự gạch dưới).
   - Ví dụ: ""Java is ... language"" -> ""Java is ____ language"".
3. [Xử lý Options]:
   - Tách các đáp án A/B/C/D.
   - Nhận diện đáp án đúng qua dấu hiệu (in đậm, dấu *, Answer Key). Nếu không tìm thấy dấu hiệu, để isCorrect = false.

OUTPUT FORMAT (JSON ARRAY ONLY):
[
  {{
    ""content"": ""Nội dung câu hỏi đã chuẩn hóa ____"",
    ""options"": [
      {{ ""id"": ""a"", ""text"": ""Option A"", ""isCorrect"": false }},
      {{ ""id"": ""b"", ""text"": ""Option B"", ""isCorrect"": true }}
    ],
    ""difficulty"": 1,
    ""points"": 1.0
  }}
]
Tuyệt đối không trả về Markdown (```json), chỉ trả về Raw JSON."
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
