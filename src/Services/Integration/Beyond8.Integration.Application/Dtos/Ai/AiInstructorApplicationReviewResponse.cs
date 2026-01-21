using System;

namespace Beyond8.Integration.Application.Dtos.Ai;

using System.Text.Json.Serialization;

public class AiInstructorApplicationReviewResponse
{
    public bool IsAccepted { get; set; }
    public int TotalScore { get; set; } = 0;
    public string? FeedbackSummary { get; set; }
    public List<SectionDetail> Details { get; set; } = [];

    public string? AdditionalFeedback { get; set; }
}

public class SectionDetail
{
    public string SectionName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SectionStatus Status { get; set; }
    public int Score { get; set; } = 0;
    public List<string> Issues { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
}

public enum SectionStatus
{
    Valid,
    Warning,
    Invalid
}