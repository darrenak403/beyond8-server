using Beyond8.Assessment.Application.Clients.Learning;
using Beyond8.Assessment.Application.Services.Implements;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Assessment.Infrastructure.Repositories.Implements;
using Beyond8.Common.Utilities;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Beyond8.IntegrationTests.Common;

public sealed class StudentLearningTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public StudentLearningTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public StudentLearningTestContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseSqlite(_connection)
            .Options;

        var dbContext = new AssessmentDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var instructorId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var (quizId, questionIds) = SeedQuizWithQuestions(dbContext, instructorId);
        var assignmentId = SeedAssignment(dbContext, instructorId);

        var unitOfWork = new UnitOfWork(dbContext);

        var publishEndpointMock = new Mock<IPublishEndpoint>();
        publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var learningClientMock = new Mock<ILearningClient>();
        learningClientMock
            .Setup(x => x.IsUserEnrolledInCourseAsync(It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "enrolled"));
        learningClientMock
            .Setup(x => x.HasCertificateForCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ApiResponse<bool>.SuccessResponse(false, "no cert"));

        var reassignServiceMock = new Mock<IReassignService>();
        reassignServiceMock
            .Setup(x => x.RecordQuizResetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        reassignServiceMock
            .Setup(x => x.RecordAssignmentResetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        var loggerFactory = LoggerFactory.Create(builder => { });

        var quizAttemptService = new QuizAttemptService(
            loggerFactory.CreateLogger<QuizAttemptService>(),
            unitOfWork,
            publishEndpointMock.Object,
            learningClientMock.Object,
            reassignServiceMock.Object);

        var assignmentSubmissionService = new AssignmentSubmissionService(
            loggerFactory.CreateLogger<AssignmentSubmissionService>(),
            unitOfWork,
            publishEndpointMock.Object,
            learningClientMock.Object,
            reassignServiceMock.Object);

        return new StudentLearningTestContext(
            dbContext,
            quizAttemptService,
            assignmentSubmissionService,
            publishEndpointMock,
            instructorId,
            studentId,
            quizId,
            questionIds,
            assignmentId);
    }

    private static (Guid QuizId, List<Guid> QuestionIds) SeedQuizWithQuestions(AssessmentDbContext dbContext, Guid instructorId)
    {
        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Content = "What is the correct answer to question 1?",
            Options = """[{"id":"opt-a","text":"Answer A","isCorrect":true},{"id":"opt-b","text":"Answer B","isCorrect":false}]""",
            Tags = """["test"]"""
        };

        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Content = "What is the correct answer to question 2?",
            Options = """[{"id":"opt-a","text":"Answer A","isCorrect":true},{"id":"opt-b","text":"Answer B","isCorrect":false}]""",
            Tags = """["test"]"""
        };

        dbContext.Questions.AddRange(question1, question2);
        dbContext.SaveChanges();

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Title = "Student Learning Flow Test Quiz",
            MaxAttempts = 2,
            IsActive = true,
            PassScorePercent = 70,
            ShuffleQuestions = false,
            TotalPoints = 100
        };

        dbContext.Quizzes.Add(quiz);
        dbContext.SaveChanges();

        var quizQuestion1 = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            QuestionId = question1.Id,
            OrderIndex = 0
        };

        var quizQuestion2 = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            QuestionId = question2.Id,
            OrderIndex = 1
        };

        dbContext.QuizQuestions.AddRange(quizQuestion1, quizQuestion2);
        dbContext.SaveChanges();

        return (quiz.Id, [question1.Id, question2.Id]);
    }

    private static Guid SeedAssignment(AssessmentDbContext dbContext, Guid instructorId)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Title = "Student Learning Flow Test Assignment",
            Description = "Design a system architecture for a REST API.",
            SubmissionType = AssignmentSubmissionType.Text,
            GradingMode = GradingMode.AiAssisted,
            MaxSubmissions = 2,
            TotalPoints = 100,
            PassScorePercent = 60,
            MaxTextLength = 1000
        };

        dbContext.Assignments.Add(assignment);
        dbContext.SaveChanges();

        return assignment.Id;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}

public sealed class StudentLearningTestContext(
    AssessmentDbContext dbContext,
    QuizAttemptService quizAttemptService,
    AssignmentSubmissionService assignmentSubmissionService,
    Mock<IPublishEndpoint> publishEndpointMock,
    Guid instructorId,
    Guid studentId,
    Guid quizId,
    List<Guid> questionIds,
    Guid assignmentId) : IDisposable
{
    public AssessmentDbContext DbContext { get; } = dbContext;
    public QuizAttemptService QuizAttemptService { get; } = quizAttemptService;
    public AssignmentSubmissionService AssignmentSubmissionService { get; } = assignmentSubmissionService;
    public Mock<IPublishEndpoint> PublishEndpointMock { get; } = publishEndpointMock;
    public Guid InstructorId { get; } = instructorId;
    public Guid StudentId { get; } = studentId;
    public Guid QuizId { get; } = quizId;
    public List<Guid> QuestionIds { get; } = questionIds;
    public Guid AssignmentId { get; } = assignmentId;

    public void Dispose()
    {
        DbContext.Dispose();
    }
}
