using Beyond8.Learning.Application.Dtos.CourseReview;
using Beyond8.Learning.Domain.Entities;

namespace Beyond8.Learning.Application.Mappings
{
    public static class CourseReviewMappings
    {
        public static CourseReviewResponse ToResponse(this CourseReview entity)
        {
            return new CourseReviewResponse
            {
                Id = entity.Id,
                CourseId = entity.CourseId,
                UserId = entity.UserId,
                EnrollmentId = entity.EnrollmentId,
                Rating = entity.Rating,
                Review = entity.Review,
                ContentQuality = entity.ContentQuality,
                InstructorQuality = entity.InstructorQuality,
                ValueForMoney = entity.ValueForMoney,
                IsVerifiedPurchase = entity.IsVerifiedPurchase,
                IsPublished = entity.IsPublished,
                HelpfulCount = entity.HelpfulCount,
                NotHelpfulCount = entity.NotHelpfulCount,
                IsFlagged = entity.IsFlagged,
                FlagReason = entity.FlagReason,
                CreatedAt = entity.CreatedAt
            };
        }

        public static CourseReview ToEntity(this CreateCourseReviewRequest request, Guid userId)
        {
            return new CourseReview
            {
                CourseId = request.CourseId,
                UserId = userId,
                EnrollmentId = request.EnrollmentId,
                Rating = request.Rating,
                Review = request.Review,
                ContentQuality = request.ContentQuality,
                InstructorQuality = request.InstructorQuality,
                ValueForMoney = request.ValueForMoney,
            };
        }
    }
}