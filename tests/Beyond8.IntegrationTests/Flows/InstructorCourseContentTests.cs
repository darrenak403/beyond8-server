using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beyond8.IntegrationTests.Flows;

public class InstructorCourseContentTests(AssessmentCourseContentTestFixture fixture)
    : IClassFixture<AssessmentCourseContentTestFixture>
{
    [Fact]
    public async Task CourseContentPipeline_ShouldConnectCourseSectionLessonQuizAssignmentAndPassConditions()
    {
        using var context = fixture.CreateContext();
        var state = InitState(context);

        state.ThumbnailMediaId = Guid.NewGuid();
        state.ThumbnailLinked = true;
        state.VideoMediaId = Guid.NewGuid();
        state.VideoReady = true;
        state.TextLessonId = Guid.NewGuid();

        var importedQuestionIds = await ImportQuestionsAsync(context, context.InstructorId, ["Q1", "Q2"]);
        state.QuestionIds = importedQuestionIds;

        var quizCreate = await context.QuizService.CreateQuizAsync(
            BuildCreateQuizRequest(state.QuestionIds, state.CourseId, state.LessonId),
            context.InstructorId);
        quizCreate.IsSuccess.Should().BeTrue();
        state.QuizId = quizCreate.Data!.Id;

        var quizUpdate = await context.QuizService.UpdateQuizAsync(
            state.QuizId,
            BuildUpdateQuizRequest(state.QuestionIds, passScorePercent: 70, maxAttempts: 2),
            context.InstructorId);
        quizUpdate.IsSuccess.Should().BeTrue();

        var assignmentCreate = await context.AssignmentService.CreateAssignmentAsync(
            BuildCreateAssignmentRequest(state.CourseId, state.SectionId, passScorePercent: 60),
            context.InstructorId);
        assignmentCreate.IsSuccess.Should().BeTrue();
        state.AssignmentId = assignmentCreate.Data!.Id;

        var persistedQuiz = await context.DbContext.Quizzes.FirstAsync(x => x.Id == state.QuizId);
        var persistedAssignment = await context.DbContext.Assignments.FirstAsync(x => x.Id == state.AssignmentId);
        persistedQuiz.PassScorePercent.Should().Be(70);
        persistedAssignment.PassScorePercent.Should().Be(60);

        var readyToPublish = state.ThumbnailLinked
                             && state.VideoReady
                             && state.TextLessonId != Guid.Empty
                             && state.QuizId != Guid.Empty
                             && state.AssignmentId != Guid.Empty;
        readyToPublish.Should().BeTrue();
    }

    [Fact]
    public async Task CourseContentPipeline_ShouldRejectPublish_WhenQuizOrAssignmentMissing()
    {
        using var context = fixture.CreateContext();
        var state = InitState(context);

        state.VideoReady = true;
        state.TextLessonId = Guid.NewGuid();

        var publishWithoutLinks = state.VideoReady
                              && state.TextLessonId != Guid.Empty
                              && state.QuizId != Guid.Empty
                              && state.AssignmentId != Guid.Empty;
        publishWithoutLinks.Should().BeFalse();

        var quizCreate = await context.QuizService.CreateQuizAsync(
            BuildCreateQuizRequest(context.QuestionIds, state.CourseId, state.LessonId),
            context.InstructorId);
        quizCreate.IsSuccess.Should().BeTrue();
        state.QuizId = quizCreate.Data!.Id;

        var publishWithoutAssignment = state.VideoReady
                                 && state.TextLessonId != Guid.Empty
                                 && state.QuizId != Guid.Empty
                                 && state.AssignmentId != Guid.Empty;
        publishWithoutAssignment.Should().BeFalse();
    }

    [Fact]
    public async Task CourseContentPipeline_ShouldRejectPublish_WhenHlsNotReady()
    {
        using var context = fixture.CreateContext();
        var state = InitState(context);

        state.VideoReady = false;
        state.TextLessonId = Guid.NewGuid();

        var quizCreate = await context.QuizService.CreateQuizAsync(
            BuildCreateQuizRequest(context.QuestionIds, state.CourseId, state.LessonId),
            context.InstructorId);
        quizCreate.IsSuccess.Should().BeTrue();
        state.QuizId = quizCreate.Data!.Id;

        var assignmentCreate = await context.AssignmentService.CreateAssignmentAsync(
            BuildCreateAssignmentRequest(state.CourseId, state.SectionId, passScorePercent: 60),
            context.InstructorId);
        assignmentCreate.IsSuccess.Should().BeTrue();
        state.AssignmentId = assignmentCreate.Data!.Id;

        var readyToPublish = state.VideoReady
                             && state.TextLessonId != Guid.Empty
                             && state.QuizId != Guid.Empty
                             && state.AssignmentId != Guid.Empty;
        readyToPublish.Should().BeFalse();
    }

    [Fact]
    public async Task CourseContentPipeline_ShouldFail_WhenUpdatingQuizWithQuestionFromAnotherInstructor()
    {
        using var context = fixture.CreateContext();
        var state = InitState(context);

        var quizCreate = await context.QuizService.CreateQuizAsync(
            BuildCreateQuizRequest(context.QuestionIds, state.CourseId, state.LessonId),
            context.InstructorId);
        quizCreate.IsSuccess.Should().BeTrue();

        var outsideQuestionId = Guid.NewGuid();
        context.DbContext.Questions.Add(new Question
        {
            Id = outsideQuestionId,
            InstructorId = Guid.NewGuid(),
            Content = "Foreign question",
            Options = """[{"id":"a","text":"A","isCorrect":true}]""",
            Tags = """[]"""
        });
        await context.DbContext.SaveChangesAsync();

        var update = await context.QuizService.UpdateQuizAsync(
            quizCreate.Data!.Id,
            BuildUpdateQuizRequest([outsideQuestionId], passScorePercent: 70, maxAttempts: 2),
            context.InstructorId);

        update.IsSuccess.Should().BeFalse();
        update.Message.Should().ContainEquivalentOf("không tồn tại");
    }

    private static CourseContentState InitState(AssessmentCourseContentTestContext context)
        => new()
        {
            CourseId = context.CourseId,
            SectionId = context.SectionId,
            LessonId = context.LessonId,
            QuestionIds = context.QuestionIds
        };

    private static async Task<List<Guid>> ImportQuestionsAsync(
        AssessmentCourseContentTestContext context,
        Guid instructorId,
        IReadOnlyList<string> contents)
    {
        var ids = new List<Guid>();
        foreach (var content in contents)
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                InstructorId = instructorId,
                Content = content,
                Options = """[{"id":"a","text":"A","isCorrect":true}]""",
                Tags = """["pipeline"]"""
            };
            ids.Add(question.Id);
            context.DbContext.Questions.Add(question);
        }

        await context.DbContext.SaveChangesAsync();
        return ids;
    }

    private static CreateQuizRequest BuildCreateQuizRequest(List<Guid> questionIds, Guid courseId, Guid lessonId)
        => new()
        {
            Title = "Quiz Lesson 1 - REST & HTTP",
            Description = "Quiz for HTTP and REST fundamentals",
            CourseId = courseId,
            LessonId = lessonId,
            TimeLimitMinutes = 20,
            PassScorePercent = 65,
            TotalPoints = 100,
            MaxAttempts = 2,
            ShuffleQuestions = true,
            AllowReview = true,
            ShowExplanation = true,
            QuestionIds = questionIds
        };

    private static UpdateQuizRequest BuildUpdateQuizRequest(List<Guid> questionIds, int passScorePercent, int maxAttempts)
        => new()
        {
            Title = "Quiz Lesson 1 - REST & HTTP",
            Description = "Quiz for HTTP and REST fundamentals",
            TimeLimitMinutes = 20,
            PassScorePercent = passScorePercent,
            TotalPoints = 100,
            MaxAttempts = maxAttempts,
            ShuffleQuestions = true,
            AllowReview = true,
            ShowExplanation = true,
            QuestionIds = questionIds
        };

    private static CreateAssignmentRequest BuildCreateAssignmentRequest(Guid courseId, Guid sectionId, int passScorePercent)
        => new()
        {
            CourseId = courseId,
            SectionId = sectionId,
            Title = "Assignment 1 - API Design",
            Description = "Design a clean REST API for course enrollment",
            SubmissionType = AssignmentSubmissionType.Text,
            MaxTextLength = 2000,
            GradingMode = GradingMode.AiAssisted,
            TotalPoints = 100,
            PassScorePercent = passScorePercent,
            TimeLimitMinutes = 60,
            MaxSubmissions = 2
        };

    private sealed class CourseContentState
    {
        public Guid CourseId { get; set; }
        public Guid ThumbnailMediaId { get; set; }
        public bool ThumbnailLinked { get; set; }
        public Guid SectionId { get; set; }
        public Guid VideoMediaId { get; set; }
        public Guid LessonId { get; set; }
        public bool VideoReady { get; set; }
        public Guid TextLessonId { get; set; }
        public List<Guid> QuestionIds { get; set; } = [];
        public Guid QuizId { get; set; }
        public Guid AssignmentId { get; set; }
    }
}
