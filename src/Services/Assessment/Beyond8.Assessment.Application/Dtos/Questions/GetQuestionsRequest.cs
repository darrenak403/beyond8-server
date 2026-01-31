using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.Questions;

public class GetQuestionsRequest : PaginationRequest
{
    public string? Tag { get; set; }
}
