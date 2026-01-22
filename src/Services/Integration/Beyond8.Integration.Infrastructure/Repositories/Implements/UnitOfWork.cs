using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements;

public class UnitOfWork(IntegrationDbContext context) : BaseUnitOfWork<IntegrationDbContext>(context), IUnitOfWork
{
    private IMediaFileRepository? _mediaFileRepository;
    private IAiUsageRepository? _aiUsageRepository;
    private IAiPromptRepository? _aiPromptRepository;
    private INotificationRepository? _notificationRepository;
    public IMediaFileRepository MediaFileRepository => _mediaFileRepository ??= new MediaFileRepository(context);
    public IAiUsageRepository AiUsageRepository => _aiUsageRepository ??= new AiUsageRepository(context);
    public IAiPromptRepository AiPromptRepository => _aiPromptRepository ??= new AiPromptRepository(context);
    public INotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(context);
}
