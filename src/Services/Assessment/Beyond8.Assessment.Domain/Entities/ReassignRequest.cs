using System.ComponentModel.DataAnnotations;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class ReassignRequest : BaseEntity
{
    public ReassignType Type { get; set; }

    public Guid SourceId { get; set; }

    public Guid StudentId { get; set; }

    public ReassignRequestReason Reason { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public ReassignRequestStatus Status { get; set; } = ReassignRequestStatus.Pending;

    public DateTime RequestedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
