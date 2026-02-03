namespace Beyond8.Assessment.Domain.JSONFields;

/// <summary>
/// Structure for AI grading feedback stored in AssignmentSubmission.AiFeedback
/// </summary>
public class AiFeedbackResult
{
    /// <summary>
    /// Overall feedback summary from AI
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Detailed feedback for each rubric criteria
    /// </summary>
    public List<CriteriaFeedback> CriteriaFeedbacks { get; set; } = [];

    /// <summary>
    /// Strengths identified in the submission
    /// </summary>
    public List<string> Strengths { get; set; } = [];

    /// <summary>
    /// Areas that need improvement
    /// </summary>
    public List<string> Improvements { get; set; } = [];

    /// <summary>
    /// Suggestions for the student
    /// </summary>
    public List<string> Suggestions { get; set; } = [];
}

/// <summary>
/// Feedback for a single rubric criteria
/// </summary>
public class CriteriaFeedback
{
    /// <summary>
    /// Name of the criteria (e.g., "Code Quality", "Completeness")
    /// </summary>
    public string CriteriaName { get; set; } = string.Empty;

    /// <summary>
    /// Score awarded for this criteria
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Maximum possible score for this criteria
    /// </summary>
    public decimal MaxScore { get; set; }

    /// <summary>
    /// Detailed feedback for this criteria
    /// </summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// Level achieved (e.g., "Excellent", "Good", "Fair", "Poor")
    /// </summary>
    public string Level { get; set; } = string.Empty;
}
