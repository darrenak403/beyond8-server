namespace Beyond8.Assessment.Domain.JSONFields;

public class AiFeedbackResult
{

    public string Summary { get; set; } = string.Empty;

    public List<CriteriaFeedback> CriteriaFeedbacks { get; set; } = [];

    public List<string> Strengths { get; set; } = [];

    public List<string> Improvements { get; set; } = [];


    public List<string> Suggestions { get; set; } = [];
}


public class CriteriaFeedback
{

    public string CriteriaName { get; set; } = string.Empty;

    public decimal Score { get; set; }

    public decimal MaxScore { get; set; }

    public string Feedback { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;
}
