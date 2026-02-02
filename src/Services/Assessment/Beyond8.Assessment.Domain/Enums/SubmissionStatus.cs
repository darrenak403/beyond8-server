namespace Beyond8.Assessment.Domain.Enums;

public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    AiGrading = 2,
    AiGraded = 3,
    ManualReview = 4,
    Graded = 5,
    ReturnedForRevision = 6
}
