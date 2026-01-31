using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.Questions;

public class GetQuestionsRequest : PaginationRequest
{
    /// <summary>
    /// Lọc câu hỏi theo tag. Null hoặc rỗng = lấy tất cả.
    /// </summary>
    public string? Tag { get; set; }
}
