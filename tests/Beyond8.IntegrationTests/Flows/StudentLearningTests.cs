using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Events.Assessment;
using Beyond8.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beyond8.IntegrationTests.Flows;

public class StudentLearningTests(StudentLearningTestFixture fixture)
    : IClassFixture<StudentLearningTestFixture>
{
    [Fact]
    public async Task StudentLearningPipeline_ShouldCompleteD5ToD7QuizAndAssignmentFlow()
    {
        using var context = fixture.CreateContext();

        var startQuiz = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);
        startQuiz.IsSuccess.Should().BeTrue();

        var answers = context.QuestionIds.ToDictionary(
            id => id.ToString(),
            _ => new List<string> { "opt-a" });
        var submitQuiz = await context.QuizAttemptService.SubmitQuizAttemptAsync(
            startQuiz.Data!.AttemptId,
            new SubmitQuizRequest { Answers = answers, TimeSpentSeconds = 120 },
            context.StudentId);
        submitQuiz.IsSuccess.Should().BeTrue();

        var submitAssignment = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            context.AssignmentId,
            new CreateSubmissionRequest { TextContent = "Assessment pipeline submission." },
            context.StudentId);
        submitAssignment.IsSuccess.Should().BeTrue();
        submitAssignment.Data!.Status.Should().Be(SubmissionStatus.AiGrading);

        var submission = await context.DbContext.AssignmentSubmissions.FirstAsync(s => s.Id == submitAssignment.Data.Id);
        submission.Status = SubmissionStatus.AiGraded;
        context.DbContext.AssignmentSubmissions.Update(submission);
        await context.DbContext.SaveChangesAsync();

        var gradeResult = await context.AssignmentSubmissionService.InstructorGradingSubmissionAsync(
            submitAssignment.Data.Id,
            new GradeSubmissionRequest { FinalScore = 85, InstructorFeedback = "Good work." },
            context.InstructorId);
        gradeResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task D5_StartQuizAttempt_WithValidQuiz_ShouldReturnAttemptIdAndQuestions()
    {
        using var context = fixture.CreateContext();

        var result = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AttemptId.Should().NotBeEmpty();
        result.Data.Questions.Should().HaveCount(2);
    }

    [Fact]
    public async Task D5_SubmitQuizAttempt_WhenSameAttemptSubmittedTwice_ShouldRejectSecondSubmission()
    {
        using var context = fixture.CreateContext();

        var startResult = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);
        startResult.IsSuccess.Should().BeTrue();
        var attemptId = startResult.Data!.AttemptId;

        var answers = context.QuestionIds.ToDictionary(
            id => id.ToString(),
            _ => new List<string> { "opt-a" });
        var submitRequest = new SubmitQuizRequest { Answers = answers, TimeSpentSeconds = 60 };

        var firstSubmit = await context.QuizAttemptService.SubmitQuizAttemptAsync(attemptId, submitRequest, context.StudentId);
        firstSubmit.IsSuccess.Should().BeTrue();

        var secondSubmit = await context.QuizAttemptService.SubmitQuizAttemptAsync(attemptId, submitRequest, context.StudentId);
        secondSubmit.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task D5_StartQuizAttempt_WhenStudentExceedsMaxAttempts_ShouldRejectNewAttempt()
    {
        using var context = fixture.CreateContext();

        var answers = context.QuestionIds.ToDictionary(
            id => id.ToString(),
            _ => new List<string> { "opt-a" });
        var submitRequest = new SubmitQuizRequest { Answers = answers, TimeSpentSeconds = 60 };

        var start1 = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);
        var submit1 = await context.QuizAttemptService.SubmitQuizAttemptAsync(start1.Data!.AttemptId, submitRequest, context.StudentId);
        submit1.IsSuccess.Should().BeTrue();

        var start2 = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);
        var submit2 = await context.QuizAttemptService.SubmitQuizAttemptAsync(start2.Data!.AttemptId, submitRequest, context.StudentId);
        submit2.IsSuccess.Should().BeTrue();

        var start3 = await context.QuizAttemptService.CreateQuizAttemptAsync(context.QuizId, context.StudentId);
        start3.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task D6_SubmitAssignment_WithValidTextContent_ShouldTransitionToAiGradingAndPublishEvent()
    {
        using var context = fixture.CreateContext();

        var result = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            context.AssignmentId,
            new CreateSubmissionRequest { TextContent = "My design involves layered REST API." },
            context.StudentId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(SubmissionStatus.AiGrading);

        context.PublishEndpointMock.Verify(
            x => x.Publish(It.IsAny<AssignmentSubmittedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task D7_PollSubmissionStatus_AfterInstructorGrades_ShouldReturnGradedStatusWithScoreAndFeedback()
    {
        using var context = fixture.CreateContext();

        var submitResult = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            context.AssignmentId,
            new CreateSubmissionRequest { TextContent = "RESTful conventions." },
            context.StudentId);

        submitResult.IsSuccess.Should().BeTrue();
        var submissionId = submitResult.Data!.Id;

        var submission = await context.DbContext.AssignmentSubmissions.FirstAsync(s => s.Id == submissionId);
        submission.Status = SubmissionStatus.AiGraded;
        submission.AiScore = 78;
        submission.AiFeedback = "Solid understanding.";
        context.DbContext.AssignmentSubmissions.Update(submission);
        await context.DbContext.SaveChangesAsync();

        var gradeResult = await context.AssignmentSubmissionService.InstructorGradingSubmissionAsync(
            submissionId,
            new GradeSubmissionRequest { FinalScore = 85, InstructorFeedback = "Well-structured." },
            context.InstructorId);
        gradeResult.IsSuccess.Should().BeTrue();

        var pollResult = await context.AssignmentSubmissionService.GetSubmissionByIdAsync(submissionId, context.StudentId);
        pollResult.IsSuccess.Should().BeTrue();
        pollResult.Data!.FinalScore.Should().Be(85);
        pollResult.Data.InstructorFeedback.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task D6_SubmitAssignment_WhenPreviousSubmissionStillPending_ShouldRejectWithPendingMessage()
    {
        using var context = fixture.CreateContext();

        var firstResult = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            context.AssignmentId,
            new CreateSubmissionRequest { TextContent = "First submission - pending." },
            context.StudentId);
        firstResult.IsSuccess.Should().BeTrue();

        var secondResult = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            context.AssignmentId,
            new CreateSubmissionRequest { TextContent = "Second attempt while pending." },
            context.StudentId);

        secondResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task D6_SubmitAssignment_WhenDeadlineExceeded_ShouldFailWithExpiredMessage()
    {
        using var context = fixture.CreateContext();

        var expiredAssignment = new Assignment
        {
            Id = Guid.NewGuid(),
            InstructorId = context.InstructorId,
            Title = "Expired Assignment",
            Description = "Deadline passed.",
            SubmissionType = AssignmentSubmissionType.Text,
            GradingMode = GradingMode.AiAssisted,
            MaxSubmissions = 3,
            TotalPoints = 100,
            PassScorePercent = 60,
            TimeLimitMinutes = 1,
            MaxTextLength = 1000
        };

        context.DbContext.Assignments.Add(expiredAssignment);
        await context.DbContext.SaveChangesAsync();
        await context.DbContext.Assignments
            .Where(a => a.Id == expiredAssignment.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.CreatedAt, DateTime.UtcNow.AddHours(-2)));

        var result = await context.AssignmentSubmissionService.CreateSubmissionAsync(
            expiredAssignment.Id,
            new CreateSubmissionRequest { TextContent = "Late submission." },
            context.StudentId);

        result.IsSuccess.Should().BeFalse();
    }
}
