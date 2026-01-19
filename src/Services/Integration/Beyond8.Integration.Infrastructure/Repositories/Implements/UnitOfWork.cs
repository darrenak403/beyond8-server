using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements;

public class UnitOfWork(IntegrationDbContext context) : BaseUnitOfWork<IntegrationDbContext>(context), IUnitOfWork
{
    private IMediaFileRepository? _mediaFileRepository;

    public IMediaFileRepository MediaFileRepository => _mediaFileRepository ??= new MediaFileRepository(context);
}
