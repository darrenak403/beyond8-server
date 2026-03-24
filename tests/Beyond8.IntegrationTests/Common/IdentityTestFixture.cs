using Beyond8.Common.Events.Identity;
using Beyond8.Identity.Application.Services.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Infrastructure.Data;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Beyond8.IntegrationTests.Common;

public sealed class IdentityTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public IdentityTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public InstructorTestContext CreateInstructorContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite(_connection)
            .Options;

        var dbContext = new IdentityDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var studentUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        SeedRolesAndUsers(dbContext, studentUserId, adminUserId);

        var unitOfWork = new UnitOfWork(dbContext);
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<InstructorProfileSubmittedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<InstructorApprovalEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FrontendUrl"] = "http://localhost:5173"
            })
            .Build();

        var logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<InstructorService>();

        var service = new InstructorService(logger, unitOfWork, publishEndpointMock.Object, config);

        return new InstructorTestContext(
            dbContext,
            service,
            publishEndpointMock,
            studentUserId,
            adminUserId);
    }

    private static void SeedRolesAndUsers(IdentityDbContext dbContext, Guid studentUserId, Guid adminUserId)
    {
        var studentRole = new Role { Id = Guid.NewGuid(), Code = "ROLE_STUDENT", Name = "Student" };
        var instructorRole = new Role { Id = Guid.NewGuid(), Code = "ROLE_INSTRUCTOR", Name = "Instructor" };
        var adminRole = new Role { Id = Guid.NewGuid(), Code = "ROLE_ADMIN", Name = "Admin" };

        var studentUser = new User
        {
            Id = studentUserId,
            Email = "studentA@example.com",
            FullName = "Student A",
            PasswordHash = "hashed"
        };

        var adminUser = new User
        {
            Id = adminUserId,
            Email = "adminA@example.com",
            FullName = "Admin A",
            PasswordHash = "hashed"
        };

        dbContext.Roles.AddRange(studentRole, instructorRole, adminRole);
        dbContext.Users.AddRange(studentUser, adminUser);
        dbContext.UserRoles.AddRange(
            new UserRole { UserId = studentUserId, RoleId = studentRole.Id },
            new UserRole { UserId = adminUserId, RoleId = adminRole.Id });
        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}

public sealed class InstructorTestContext(
    IdentityDbContext dbContext,
    InstructorService instructorService,
    Mock<IPublishEndpoint> publishEndpointMock,
    Guid studentUserId,
    Guid adminUserId) : IDisposable
{
    public IdentityDbContext DbContext { get; } = dbContext;
    public InstructorService Service { get; } = instructorService;
    public Mock<IPublishEndpoint> PublishEndpointMock { get; } = publishEndpointMock;
    public Guid StudentUserId { get; } = studentUserId;
    public Guid AdminUserId { get; } = adminUserId;

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
