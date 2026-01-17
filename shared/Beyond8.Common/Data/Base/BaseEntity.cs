using System;
using System.ComponentModel.DataAnnotations;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Common.Data.Base;

public abstract class BaseEntity : IAuditableEntity, ISoftDeleteEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

