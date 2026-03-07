using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.Questions;

public class GetQuestionsRequest : PaginationRequest
{
    public string? Tag { get; set; }
    public string? Keyword { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
}
