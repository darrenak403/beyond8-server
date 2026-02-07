namespace Beyond8.Learning.Application.Dtos.CourseReview
{
    public class CourseReviewResponse
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public Guid EnrollmentId { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }
        public int? ContentQuality { get; set; }
        public int? InstructorQuality { get; set; }
        public int? ValueForMoney { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsPublished { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public Guid? ModeratedBy { get; set; }
        public DateTime? ModeratedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}