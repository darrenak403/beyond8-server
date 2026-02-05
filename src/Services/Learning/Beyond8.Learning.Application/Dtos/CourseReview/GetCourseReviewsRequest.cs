using Beyond8.Common.Utilities;

namespace Beyond8.Learning.Application.Dtos.CourseReview;

public class GetCourseReviewsRequest : PaginationRequest
{
    public Guid CourseId { get; set; }
}
