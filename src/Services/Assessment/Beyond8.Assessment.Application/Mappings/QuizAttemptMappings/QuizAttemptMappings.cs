using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Assessment.Application.Helpers;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;

namespace Beyond8.Assessment.Application.Mappings.QuizAttemptMappings;

public static class QuizAttemptMappings
{
    public static QuizAttempt ToEntity(
        Guid studentId,
        Guid quizId,
        int attemptNumber,
        int shuffleSeed,
        List<Guid> shuffledQuestionIds,
        Dictionary<string, List<string>> optionOrders)
    {
        return new QuizAttempt
        {
            StudentId = studentId,
            QuizId = quizId,
            AttemptNumber = attemptNumber,
            StartedAt = DateTime.UtcNow,
            ShuffleSeed = shuffleSeed,
            QuestionOrder = JsonSerializer.Serialize(shuffledQuestionIds),
            OptionOrders = JsonSerializer.Serialize(optionOrders),
            Status = QuizAttemptStatus.InProgress,
            CreatedBy = studentId
        };
    }

    public static StartQuizResponse ToStartQuizResponse(
        this QuizAttempt attempt,
        Quiz quiz,
        List<Guid> shuffledQuestionIds,
        Dictionary<Guid, Question> questionDict,
        Dictionary<string, List<string>> optionOrders)
    {
        var responseQuestions = new List<QuizQuestionForStudentResponse>();
        var orderIndex = 1;

        foreach (var questionId in shuffledQuestionIds)
        {
            if (!questionDict.TryGetValue(questionId, out var question)) continue;

            var options = JsonSerializer.Deserialize<List<QuestionOptionItem>>(question.Options) ?? [];
            var shuffledOptionIds = optionOrders[questionId.ToString()];
            var optionLookup = options.ToDictionary(o => o.Id);

            var responseOptions = shuffledOptionIds
                .Where(id => optionLookup.ContainsKey(id))
                .Select(id => new QuestionOptionForStudentResponse
                {
                    Id = optionLookup[id].Id,
                    Text = optionLookup[id].Text
                })
                .ToList();

            responseQuestions.Add(new QuizQuestionForStudentResponse
            {
                QuestionId = questionId,
                OrderIndex = orderIndex++,
                Content = question.Content,
                Type = question.Type,
                Points = question.Points,
                Options = responseOptions
            });
        }

        return new StartQuizResponse
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            QuizDescription = quiz.Description,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            TotalQuestions = responseQuestions.Count,
            TotalPoints = quiz.TotalPoints,
            PassScorePercent = quiz.PassScorePercent,
            Questions = responseQuestions
        };
    }

    public static QuizResultResponse ToResultResponse(
        this QuizAttempt attempt,
        Quiz quiz,
        List<Guid> questionOrder,
        Dictionary<Guid, Question> questionDict,
        Dictionary<string, List<string>> studentAnswers)
    {
        var questionResults = new List<QuestionResultResponse>();
        var orderIndex = 1;
        var correctCount = 0;
        var wrongCount = 0;

        foreach (var questionId in questionOrder)
        {
            if (!questionDict.TryGetValue(questionId, out var question)) continue;

            var options = JsonSerializer.Deserialize<List<QuestionOptionItem>>(question.Options) ?? [];
            var correctOptionIds = options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();

            var selectedAnswers = studentAnswers.TryGetValue(questionId.ToString(), out var answers)
                ? answers.ToHashSet()
                : [];

            var isCorrect = correctOptionIds.SetEquals(selectedAnswers);
            var earnedPoints = isCorrect ? question.Points : 0;

            if (isCorrect)
                correctCount++;
            else
                wrongCount++;

            questionResults.Add(new QuestionResultResponse
            {
                QuestionId = questionId,
                OrderIndex = orderIndex++,
                Content = question.Content,
                Type = question.Type,
                Points = question.Points,
                EarnedPoints = earnedPoints,
                IsCorrect = isCorrect,
                SelectedOptions = selectedAnswers.ToList(),
                CorrectOptions = correctOptionIds.ToList(),
                Explanation = quiz.ShowExplanation ? question.Explanation : null,
                Options = options.Select(o => new QuestionOptionResultResponse
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    IsSelected = selectedAnswers.Contains(o.Id)
                }).ToList()
            });
        }

        return new QuizResultResponse
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            Score = attempt.Score ?? 0,
            ScorePercent = attempt.ScorePercent ?? 0,
            TotalPoints = quiz.TotalPoints,
            PassScorePercent = quiz.PassScorePercent,
            IsPassed = attempt.IsPassed ?? false,
            TotalQuestions = questionOrder.Count,
            CorrectAnswers = correctCount,
            WrongAnswers = wrongCount,
            Status = attempt.Status,
            QuestionResults = quiz.AllowReview ? questionResults : null
        };
    }

    public static void UpdateForSubmission(
        this QuizAttempt attempt,
        Dictionary<string, List<string>> answers,
        int timeSpentSeconds,
        decimal totalScore,
        decimal scorePercent,
        bool isPassed,
        Guid studentId)
    {
        attempt.Answers = JsonSerializer.Serialize(answers);
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TimeSpentSeconds = timeSpentSeconds;
        attempt.Score = totalScore;
        attempt.ScorePercent = scorePercent;
        attempt.IsPassed = isPassed;
        attempt.Status = QuizAttemptStatus.Graded;
        attempt.UpdatedBy = studentId;
    }

    public static QuizResultResponse ToSubmitResultResponse(
        this QuizAttempt attempt,
        Quiz quiz,
        int totalQuestions,
        decimal totalScore,
        decimal scorePercent,
        bool isPassed,
        int correctCount,
        int wrongCount,
        List<QuestionResultResponse> questionResults)
    {
        return new QuizResultResponse
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            Score = totalScore,
            ScorePercent = scorePercent,
            TotalPoints = quiz.TotalPoints,
            PassScorePercent = quiz.PassScorePercent,
            IsPassed = isPassed,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctCount,
            WrongAnswers = wrongCount,
            Status = attempt.Status,
            QuestionResults = quiz.AllowReview ? questionResults : null
        };
    }

    public static (decimal TotalScore, int CorrectCount, int WrongCount, List<QuestionResultResponse> QuestionResults) GradeQuiz(
        List<Guid> questionOrder,
        Dictionary<Guid, Question> questionDict,
        Dictionary<string, List<string>> studentAnswers,
        bool showExplanation)
    {
        decimal totalScore = 0;
        var correctCount = 0;
        var wrongCount = 0;
        var questionResults = new List<QuestionResultResponse>();
        var orderIndex = 1;

        foreach (var questionId in questionOrder)
        {
            if (!questionDict.TryGetValue(questionId, out var question)) continue;

            var options = JsonSerializer.Deserialize<List<QuestionOptionItem>>(question.Options) ?? [];
            var correctOptionIds = options.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();

            var selectedAnswers = studentAnswers.TryGetValue(questionId.ToString(), out var answers)
                ? answers.ToHashSet()
                : [];

            var isCorrect = correctOptionIds.SetEquals(selectedAnswers);
            var earnedPoints = isCorrect ? question.Points : 0;

            if (isCorrect)
                correctCount++;
            else
                wrongCount++;

            totalScore += earnedPoints;

            questionResults.Add(new QuestionResultResponse
            {
                QuestionId = questionId,
                OrderIndex = orderIndex++,
                Content = question.Content,
                Type = question.Type,
                Points = question.Points,
                EarnedPoints = earnedPoints,
                IsCorrect = isCorrect,
                SelectedOptions = selectedAnswers.ToList(),
                CorrectOptions = correctOptionIds.ToList(),
                Explanation = showExplanation ? question.Explanation : null,
                Options = options.Select(o => new QuestionOptionResultResponse
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    IsSelected = selectedAnswers.Contains(o.Id)
                }).ToList()
            });
        }

        return (totalScore, correctCount, wrongCount, questionResults);
    }

    public static QuizAttemptSummaryResponse ToSummaryResponse(this QuizAttempt attempt, string quizTitle)
    {
        return new QuizAttemptSummaryResponse
        {
            AttemptId = attempt.Id,
            QuizId = attempt.QuizId,
            QuizTitle = quizTitle,
            AttemptNumber = attempt.AttemptNumber,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Score = attempt.Score,
            ScorePercent = attempt.ScorePercent,
            IsPassed = attempt.IsPassed,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            Status = attempt.Status
        };
    }

    public static UserQuizAttemptsResponse ToUserQuizAttemptsResponse(
        this IReadOnlyCollection<QuizAttempt> attempts,
        Quiz quiz)
    {
        var attemptsList = attempts.OrderByDescending(a => a.AttemptNumber).ToList();
        var completedAttempts = attemptsList.Where(a => a.Status == QuizAttemptStatus.Graded).ToList();

        return new UserQuizAttemptsResponse
        {
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            MaxAttempts = quiz.MaxAttempts,
            UsedAttempts = attempts.Count,
            RemainingAttempts = quiz.MaxAttempts > 0 ? Math.Max(0, quiz.MaxAttempts - attempts.Count) : -1,
            BestScore = completedAttempts.Count > 0 ? completedAttempts.Max(a => a.ScorePercent) : null,
            LatestScore = completedAttempts.FirstOrDefault()?.ScorePercent,
            Attempts = attemptsList.Select(a => a.ToSummaryResponse(quiz.Title)).ToList()
        };
    }

    public static Dictionary<string, List<string>> GenerateOptionOrders(
        List<Guid> questionIds,
        Dictionary<Guid, Question> questionDict,
        int shuffleSeed)
    {
        var optionOrders = new Dictionary<string, List<string>>();

        foreach (var questionId in questionIds)
        {
            if (!questionDict.TryGetValue(questionId, out var question)) continue;

            var options = JsonSerializer.Deserialize<List<QuestionOptionItem>>(question.Options) ?? [];
            var optionIds = options.Select(o => o.Id).ToList();
            var optionSeed = shuffleSeed ^ questionId.GetHashCode();
            var shuffledOptionIds = FisherYatesShuffler.ShuffleCopy(optionIds, optionSeed);
            optionOrders[questionId.ToString()] = shuffledOptionIds;
        }

        return optionOrders;
    }
}
