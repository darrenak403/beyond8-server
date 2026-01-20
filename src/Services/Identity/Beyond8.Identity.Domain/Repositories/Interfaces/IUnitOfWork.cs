using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Identity.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository UserRepository { get; }
    IInstructorProfileRepository InstructorProfileRepository { get; }
}
