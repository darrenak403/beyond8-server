using System;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public CourseLevel Level { get; set; }
    public string Language { get; set; } = "vi-VN";
    public decimal Price { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalSections { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDurationMinutes { get; set; }
    public decimal? AvgRating { get; set; }
    public int TotalReviews { get; set; }
    public List<string> Outcomes { get; set; } = [];
    public List<string>? Requirements { get; set; }
    public List<string>? TargetAudience { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
