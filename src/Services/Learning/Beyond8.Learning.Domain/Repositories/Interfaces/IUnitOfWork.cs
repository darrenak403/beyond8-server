using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Learning.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IEnrollmentRepository EnrollmentRepository { get; }
    ILessonProgressRepository LessonProgressRepository { get; }
    ISectionProgressRepository SectionProgressRepository { get; }
}
