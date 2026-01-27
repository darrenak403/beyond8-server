using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Repositories.Inplements;

namespace Beyond8.Identity.Infrastructure.Data
{
    public class UnitOfWork(IdentityDbContext context) : BaseUnitOfWork<IdentityDbContext>(context), IUnitOfWork
    {
        private IUserRepository? _userRepository;
        private IInstructorProfileRepository? _instructorProfileRepository;
        private IRoleRepository? _roleRepository;
        private ISubscriptionPlanRepository? _subscriptionPlanRepository;
        private IUserSubscriptionRepository? _userSubscriptionRepository;

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(context);
        public IInstructorProfileRepository InstructorProfileRepository => _instructorProfileRepository ??= new InstructorProfileRepository(context);
        public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(context);
        public ISubscriptionPlanRepository SubscriptionPlanRepository => _subscriptionPlanRepository ??= new SubscriptionPlanRepository(context);
        public IUserSubscriptionRepository UserSubscriptionRepository => _userSubscriptionRepository ??= new UserSubscriptionRepository(context);
    }
}
