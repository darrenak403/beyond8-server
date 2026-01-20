using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Repositories.Inplements;

namespace Beyond8.Identity.Infrastructure.Data;

public class UnitOfWork(IdentityDbContext context) : BaseUnitOfWork<IdentityDbContext>(context), IUnitOfWork
{
    private IUserRepository? _userRepository;
    private IInstructorProfileRepository? _instructorProfileRepository;

    public IUserRepository UserRepository => _userRepository ??= new UserRepository(context);
    public IInstructorProfileRepository InstructorProfileRepository => _instructorProfileRepository ??= new InstructorProfileRepository(context);
}
