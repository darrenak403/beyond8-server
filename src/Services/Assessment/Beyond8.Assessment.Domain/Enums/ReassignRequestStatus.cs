namespace Beyond8.Assessment.Domain.Enums;

/// <summary>
/// Trạng thái yêu cầu reassign của học sinh.
/// </summary>
public enum ReassignRequestStatus
{
    /// <summary>Đang chờ instructor xử lý</summary>
    Pending = 0,

    /// <summary>Instructor đã đồng ý và đã reset</summary>
    Approved = 1,

    /// <summary>Instructor từ chối</summary>
    Rejected = 2
}
