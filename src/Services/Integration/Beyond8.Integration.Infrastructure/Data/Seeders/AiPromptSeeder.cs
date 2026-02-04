using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Data.Seeders
{
  public static class AiPromptSeeder
  {
    public static async Task SeedAsync(IntegrationDbContext context)
    {
      if (!await context.AiPrompts.AnyAsync())
      {
        var prompts = new List<AiPrompt>();
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

      var allPrompts = new List<AiPrompt>();
      allPrompts.AddRange(GetCourseContentPrompts());
      allPrompts.AddRange(GetAssessmentPrompts());
      allPrompts.AddRange(GetFeedbackPrompts());
      allPrompts.AddRange(GetContentAnalysisPrompts());
      allPrompts.AddRange(GetTranslationPrompts());
      allPrompts.AddRange(GetPersonalizationPrompts());
      allPrompts.AddRange(GetModerationPrompts());

      foreach (var prompt in allPrompts)
      {
        if (!await context.AiPrompts.AnyAsync(p => p.Name == prompt.Name))
        {
          await context.AiPrompts.AddAsync(prompt);
        }
      }

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
          // PROMPT 1: QUIZ GENERATION
          new AiPrompt
        {
            Name = "Quiz Generation",
            Description = "Sinh câu hỏi trắc nghiệm Bloom. Hỗ trợ điểm thập phân và tự động bù trừ tổng điểm.",
            Category = PromptCategory.Assessment,
            Tags = "Education, Content_Generation, Business_Logic",
            SystemPrompt = "Bạn là chuyên gia Khảo thí. Nhiệm vụ: Tạo JSON câu hỏi trắc nghiệm. Nguyên tắc: Tổng điểm PHẢI bằng chính xác 'MaxPoints'.",
            Version = "3.4.0",
            IsActive = true,
            MaxTokens = 4096,
            Temperature = 0.2m,
            TopP = 0.9m,
            Template = @"
INPUT:
---
CONTEXT: {Context}
---
CHỦ ĐỀ: {Query}

CẤU HÌNH:
- Số lượng: Easy={EasyCount}, Medium={MediumCount}, Hard={HardCount}.
- MAX POINTS: {MaxPoints}

QUY TẮC TAGGING (QUAN TRỌNG):
- Trong mảng ""tags"" của JSON output: Chỉ sử dụng các Tag tổng quát (High-level Category), KHÔNG dùng chủ đề chi tiết.
- Ví dụ: Dùng 'Grammar' thay vì 'Verb'; dùng 'Science' thay vì 'Quantum Physics'; dùng 'History' thay vì 'Vietnam War'.

QUY TRÌNH TÍNH ĐIỂM (AUTO-CORRECTION):
1. Trọng số: Easy=1, Medium=1.5, Hard=2.
2. Tổng trọng số W = ({EasyCount}*1) + ({MediumCount}*1.5) + ({HardCount}*2).
3. Base Unit = {MaxPoints} / W.
4. Tính sơ bộ (làm tròn 2 số thập phân): Easy=Unit, Medium=Unit*1.5, Hard=Unit*2.
5. BÙ TRỪ: Tính Delta = {MaxPoints} - (Tổng điểm sơ bộ). Cộng Delta vào câu hỏi cuối cùng để Tổng = {MaxPoints} tuyệt đối.

OUTPUT SCHEMA (RAW JSON):
{{
  ""easy"": [ 
    {{ 
      ""content"": ""..."", 
      ""type"": 0, 
      ""options"": [{{ ""id"": ""a"", ""text"": ""..."", ""isCorrect"": false }}], 
      ""difficulty"": 0, 
      ""points"": <số thập phân>,
      ""tags"": [""General_Category_1""] 
    }} 
  ],
  ""medium"": [ ... ],
  ""hard"": [ ... ]
}}"
        },

        GetAssignmentGradingPrompt(),

        // PROMPT 2: FORMAT QUIZ
        new AiPrompt
        {
            Name = "Format Quiz Questions",
            Description = "Trích xuất và chuẩn hóa câu hỏi từ tài liệu thô.",
            Category = PromptCategory.Assessment,
            Tags = "Data_Processing, Document_Utility, Standardization",
            SystemPrompt = "Bạn là chuyên gia xử lý dữ liệu. Nhiệm vụ: Trích xuất câu hỏi sang JSON và chuẩn hóa format ____.",
            Version = "1.3.0",
            IsActive = true,
            MaxTokens = 4096,
            Temperature = 0.1m,
            TopP = 0.5m,
            Template = @"
INPUT TEXT:
---
{Content}
---

NHIỆM VỤ:
1. [Lọc nhiễu]: Bỏ số trang, header/footer.
2. [Chuẩn hóa Content]: Thay thế mọi kiểu chỗ trống (..., ___, ..) bằng đúng 4 gạch dưới: ____.
3. [Options]: Tách A/B/C/D. Tự động detect isCorrect (nếu không rõ thì false).

OUTPUT (RAW JSON ARRAY):
[
  {{
    ""content"": ""Câu hỏi đã chuẩn hóa ____"",
    ""options"": [{{ ""id"": ""a"", ""text"": ""..."", ""isCorrect"": boolean }}],
    ""difficulty"": 1,
    ""points"": 1.0
  }}
]"
        }
      ];
    }

    private static AiPrompt GetAssignmentGradingPrompt()
    {
      return new AiPrompt
      {
        Name = "Assignment Grading",
        Description = "Chấm điểm bài tập dựa trên nội dung nộp, mô tả bài tập và rubric (nếu có). Trả về điểm số, nhận xét tổng quan, tiêu chí chi tiết và gợi ý cải thiện.",
        Category = PromptCategory.Assessment,
        Tags = "Education, Grading, Assignment, Rubric, Feedback",
        SystemPrompt = "Bạn là trợ lý chấm bài chuyên nghiệp. Nhiệm vụ: Đọc bài nộp của học viên, đối chiếu với yêu cầu bài tập và rubric (nếu có), chấm điểm công bằng và đưa ra nhận xét mang tính xây dựng. Trả về đúng format JSON theo OUTPUT SCHEMA. Ngôn ngữ nhận xét: Tiếng Việt.",
        Version = "1.0.0",
        IsActive = true,
        MaxTokens = 4096,
        Temperature = 0.3m,
        TopP = 0.9m,
        Template = @"
BÀI TẬP:
- Tiêu đề: {AssignmentTitle}
- Mô tả / Yêu cầu: {AssignmentDescription}
- Thang điểm tối đa: {TotalPoints}

RUBRIC / TIÊU CHÍ CHẤM (nếu có):
{RubricContent}

---
NỘI DUNG BÀI NỘP CỦA HỌC VIÊN:
---
{SubmissionContent}
---

YÊU CẦU:
1. Chấm điểm theo thang {TotalPoints}, có thể dùng số thập phân (ví dụ 7.5).
2. Điểm số (score) phải trong khoảng 0 đến {TotalPoints}.
3. Nhận xét tổng quan (summary/overallFeedback): ngắn gọn, mang tính xây dựng, bằng Tiếng Việt.
4. criteriaResults: mảng các tiêu chí đã chấm (theo rubric hoặc tự đặt tên tiêu chí hợp lý), mỗi phần tử gồm: criteriaName, score, maxScore, level (Xuất sắc/Tốt/Khá/Trung bình/Yếu/Kém), feedback.
5. strengths: mảng các điểm mạnh của bài (chuỗi).
6. improvements hoặc areasForImprovement: mảng các điểm cần cải thiện (chuỗi).
7. suggestions hoặc recommendations: mảng gợi ý cụ thể để học viên tiến bộ (chuỗi).

OUTPUT SCHEMA (chỉ trả về JSON, không markdown):
{
  ""score"": <số từ 0 đến " + @"{TotalPoints}" + @">,
  ""summary"": ""Nhận xét tổng quan ngắn gọn bằng Tiếng Việt"",
  ""criteriaResults"": [
    {
      ""criteriaName"": ""Tên tiêu chí"",
      ""score"": <số>,
      ""maxScore"": <số>,
      ""level"": ""Xuất sắc|Tốt|Khá|Trung bình|Yếu|Kém"",
      ""feedback"": ""Nhận xét cho tiêu chí này""
    }
  ],
  ""strengths"": [""Điểm mạnh 1"", ""Điểm mạnh 2""],
  ""improvements"": [""Điểm cần cải thiện 1"", ""Điểm cần cải thiện 2""],
  ""suggestions"": [""Gợi ý 1"", ""Gợi ý 2""]
}"
      };
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
