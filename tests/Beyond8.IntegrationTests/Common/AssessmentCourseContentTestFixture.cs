using Beyond8.Assessment.Application.Clients.Catalog;
using Beyond8.Assessment.Application.Clients.Learning;
using Beyond8.Assessment.Application.Services.Implements;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Assessment.Infrastructure.Repositories.Implements;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Beyond8.IntegrationTests.Common;

public sealed class AssessmentCourseContentTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public AssessmentCourseContentTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public AssessmentCourseContentTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseSqlite(_connection)
            .Options;

        var dbContext = new AssessmentDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var instructorId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var questionIds = SeedQuestionBank(dbContext, instructorId);
        var unitOfWork = new UnitOfWork(dbContext);

        var catalogServiceMock = new Mock<ICatalogService>();
        catalogServiceMock
            .Setup(x => x.UpdateQuizForLessonAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "ok"));
        catalogServiceMock
            .Setup(x => x.UpdateAssignmentForSectionAsync(It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "ok"));
        catalogServiceMock
            .Setup(x => x.IsLessonPreviewByQuizIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(false, "not preview"));

        var learningClientMock = new Mock<ILearningClient>();
        learningClientMock
            .Setup(x => x.IsUserEnrolledInCourseAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "enrolled"));
        learningClientMock
            .Setup(x => x.HasCertificateForCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(false, "no cert"));

        var publishEndpointMock = new Mock<IPublishEndpoint>();
        publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var loggerFactory = LoggerFactory.Create(builder => { });
        var quizService = new QuizService(
            loggerFactory.CreateLogger<QuizService>(),
            unitOfWork,
            catalogServiceMock.Object,
            learningClientMock.Object);

        var assignmentService = new AssignmentService(
            loggerFactory.CreateLogger<AssignmentService>(),
            unitOfWork,
            catalogServiceMock.Object,
            learningClientMock.Object,
            publishEndpointMock.Object);

        return new AssessmentCourseContentTestContext(
            dbContext,
            quizService,
            assignmentService,
            catalogServiceMock,
            instructorId,
            courseId,
            lessonId,
            sectionId,
            questionIds);
    }

    private static List<Guid> SeedQuestionBank(AssessmentDbContext dbContext, Guid instructorId)
    {
        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Content = "HTTP status code for resource creation?",
            Options = """[{"id":"a","text":"200","isCorrect":false},{"id":"b","text":"201","isCorrect":true}]""",
            Tags = """["rest","http"]"""
        };

        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Content = "Which verb should update partial resource?",
            Options = """[{"id":"a","text":"PUT","isCorrect":false},{"id":"b","text":"PATCH","isCorrect":true}]""",
            Tags = """["rest","http"]"""
        };

        dbContext.Questions.AddRange(question1, question2);
        dbContext.SaveChanges();
        return [question1.Id, question2.Id];
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}

public sealed class AssessmentCourseContentTestContext(
    AssessmentDbContext dbContext,
    QuizService quizService,
    AssignmentService assignmentService,
    Mock<ICatalogService> catalogServiceMock,
    Guid instructorId,
    Guid courseId,
    Guid lessonId,
    Guid sectionId,
    List<Guid> questionIds) : IDisposable
{
    public AssessmentDbContext DbContext { get; } = dbContext;
    public QuizService QuizService { get; } = quizService;
    public AssignmentService AssignmentService { get; } = assignmentService;
    public Mock<ICatalogService> CatalogServiceMock { get; } = catalogServiceMock;
    public Guid InstructorId { get; } = instructorId;
    public Guid CourseId { get; } = courseId;
    public Guid LessonId { get; } = lessonId;
    public Guid SectionId { get; } = sectionId;
    public List<Guid> QuestionIds { get; } = questionIds;

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
