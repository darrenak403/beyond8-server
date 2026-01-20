using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Infrastructure.Data.Seeders.Templates;

/// <summary>
/// Prompt templates for content moderation and review
/// Examples: Review instructor applications, moderate forum posts, check content quality
/// </summary>
public static class ModerationPrompts
{
    public static List<AiPrompt> GetPrompts()
    {
        return new List<AiPrompt>
        {
            new AiPrompt
            {
                Name = "Instructor Application Review",
                Description = "Review and assess instructor application submissions",
                Category = PromptCategory.Moderation,
                Template = @"Bạn là người đánh giá hồ sơ giảng viên. Hãy xem xét đơn đăng ký sau:

Thông tin ứng viên:
Tên: {applicantName}
Kinh nghiệm: {experience}
Trình độ: {education}
Chứng chỉ: {certifications}

Mô tả bản thân:
{description}

Môn học đăng ký dạy:
{subjects}

Hãy đánh giá:
1. Điểm mạnh của ứng viên
2. Điểm cần lưu ý
3. Đề xuất: Chấp nhận / Yêu cầu bổ sung / Từ chối
4. Lý do cụ thể cho đề xuất
5. Câu hỏi cần làm rõ thêm (nếu có)",
                SystemPrompt = "Bạn là chuyên gia tuyển dụng giảng viên, đánh giá công bằng và khách quan.",
                Version = "1.0",
                IsActive = true,
                Variables = @"{""applicantName"": ""tên ứng viên"", ""experience"": ""kinh nghiệm"", ""education"": ""trình độ học vấn"", ""certifications"": ""chứng chỉ"", ""description"": ""mô tả bản thân"", ""subjects"": ""môn học""}",
                MaxTokens = 2500,
                Temperature = 0.6m,
                TopP = 0.9m,
                Tags = "moderation,instructor,application-review"
            },
            // TODO: Add more moderation prompts here
            // Examples:
            // - Moderate forum discussions
            // - Check course content quality
            // - Review student reports
            // - Detect inappropriate content
        };
    }
}
