namespace Beyond8.Common.Data.Interfaces
{
    public interface IEntity
    {
        public Guid Id { get; set; }
    }

    public interface IAuditableEntity : IEntity
    {
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }

    public interface ISoftDeleteEntity : IEntity
    {
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}