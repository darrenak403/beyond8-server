namespace Beyond8.Catalog.Application.Dtos.Courses;

public class SetCourseDiscountRequest
{
    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public DateTime? DiscountEndsAt { get; set; }
}
