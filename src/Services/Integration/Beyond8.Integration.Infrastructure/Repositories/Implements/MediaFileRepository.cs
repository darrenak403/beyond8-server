using System;
using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements;

public class MediaFileRepository(IntegrationDbContext context) : PostgresRepository<MediaFile>(context), IMediaFileRepository
{
}
