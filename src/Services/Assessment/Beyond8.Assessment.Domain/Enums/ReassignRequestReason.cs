namespace Beyond8.Assessment.Domain.Enums;

/// <summary>
/// Lý do học sinh yêu cầu reassign (reset lượt làm quiz / nộp assignment).
/// </summary>
public enum ReassignRequestReason
{
    /// <summary>Lỗi kỹ thuật (mất kết nối, trình duyệt crash, ...)</summary>
    TechnicalIssue = 0,

    /// <summary>Chấm điểm không công bằng hoặc cần phúc khảo</summary>
    UnfairGrading = 1,

    /// <summary>Cần thêm cơ hội ôn tập / làm lại</summary>
    NeedMorePractice = 2,

    /// <summary>Lý do khác (ghi trong note)</summary>
    Other = 3
}
