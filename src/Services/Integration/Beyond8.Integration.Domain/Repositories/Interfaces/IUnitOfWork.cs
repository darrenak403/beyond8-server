using System;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Integration.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IMediaFileRepository MediaFileRepository { get; }
}
