using System.Text.Json;
using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Assessment.Application.Helpers;
using Beyond8.Assessment.Application.Mappings.QuizAttemptMappings;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Beyond8.Assessment.Application.Services.Implements;

public class QuizAttemptService(
    ILogger<QuizAttemptService> logger,
    IUnitOfWork unitOfWork) : IQuizAttemptService
{
    public async Task<ApiResponse<StartQuizResponse>> CreateQuizAttemptAsync(Guid quizId, Guid studentId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == quizId && q.IsActive);
            if (quiz == null)
                return ApiResponse<StartQuizResponse>.FailureResponse("Quiz không tồn tại hoặc đã bị xóa.");

            var existingAttempts = await unitOfWork.QuizAttemptRepository.GetAllAsync(
                a => a.QuizId == quizId && a.StudentId == studentId);

            var (canStart, errorMessage) = ValidateCanStartQuiz(quiz, existingAttempts);
            if (!canStart)
                return ApiResponse<StartQuizResponse>.FailureResponse(errorMessage!);

            var quizQuestions = await unitOfWork.QuizQuestionRepository.GetAllAsync(qq => qq.QuizId == quizId);
            if (quizQuestions.Count == 0)
                return ApiResponse<StartQuizResponse>.FailureResponse("Quiz không có câu hỏi nào.");

            var questionIds = quizQuestions.OrderBy(qq => qq.OrderIndex).Select(qq => qq.QuestionId).ToList();
            var questions = await unitOfWork.QuestionRepository.GetAllAsync(q => questionIds.Contains(q.Id));
            var questionDict = questions.ToDictionary(q => q.Id);

            var shuffleSeed = FisherYatesShuffler.GenerateSeed();

            var shuffledQuestionIds = quiz.ShuffleQuestions
                ? FisherYatesShuffler.ShuffleCopy(questionIds, shuffleSeed)
                : questionIds.ToList();

            var optionOrders = QuizAttemptMappings.GenerateOptionOrders(shuffledQuestionIds, questionDict, shuffleSeed);

            var attemptNumber = existingAttempts.Count + 1;
            var attempt = QuizAttemptMappings.ToEntity(
                studentId, quizId, attemptNumber, shuffleSeed, shuffledQuestionIds, optionOrders);

            await unitOfWork.QuizAttemptRepository.AddAsync(attempt);
            await unitOfWork.SaveChangesAsync();

            var response = attempt.ToStartQuizResponse(quiz, shuffledQuestionIds, questionDict, optionOrders);

            logger.LogInformation(
                "Quiz attempt created: AttemptId={AttemptId}, QuizId={QuizId}, StudentId={StudentId}, AttemptNumber={AttemptNumber}",
                attempt.Id, quizId, studentId, attemptNumber);

            return ApiResponse<StartQuizResponse>.SuccessResponse(response, "Bắt đầu làm quiz thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating quiz attempt: QuizId={QuizId}, StudentId={StudentId}", quizId, studentId);
            return ApiResponse<StartQuizResponse>.FailureResponse("Đã xảy ra lỗi khi bắt đầu làm quiz.");
        }
    }

    public async Task<ApiResponse<QuizResultResponse>> SubmitQuizAttemptAsync(Guid attemptId, SubmitQuizRequest request, Guid studentId)
    {
        try
        {
            var (attempt, quiz, errorMessage) = await ValidateAttemptForSubmissionAsync(attemptId, studentId);
            if (attempt == null || quiz == null)
                return ApiResponse<QuizResultResponse>.FailureResponse(errorMessage!);

            CheckTimeLimit(quiz, attempt, attemptId);

            var (questionOrder, questionDict) = await LoadQuestionsFromAttemptAsync(attempt);

            var (totalScore, correctCount, wrongCount, questionResults) = QuizAttemptMappings.GradeQuiz(
                questionOrder, questionDict, request.Answers, quiz.ShowExplanation);

            var maxScore = questionDict.Values.Sum(q => q.Points);
            var scorePercent = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;
            var isPassed = scorePercent >= quiz.PassScorePercent;

            attempt.UpdateForSubmission(request.Answers, request.TimeSpentSeconds, totalScore, scorePercent, isPassed, studentId);

            await unitOfWork.QuizAttemptRepository.UpdateAsync(attemptId, attempt);

            await UpdateQuizStatisticsAsync(quiz, isPassed);

            await unitOfWork.SaveChangesAsync();

            var response = attempt.ToSubmitResultResponse(
                quiz, questionOrder.Count, totalScore, scorePercent, isPassed, correctCount, wrongCount, questionResults);

            logger.LogInformation(
                "Quiz attempt submitted: AttemptId={AttemptId}, Score={Score}/{MaxScore}, Passed={IsPassed}",
                attemptId, totalScore, maxScore, isPassed);

            return ApiResponse<QuizResultResponse>.SuccessResponse(response, "Nộp bài thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting quiz attempt: AttemptId={AttemptId}", attemptId);
            return ApiResponse<QuizResultResponse>.FailureResponse("Đã xảy ra lỗi khi nộp bài.");
        }
    }

    public async Task<ApiResponse<bool>> AutoSaveQuizAttemptAsync(Guid attemptId, AutoSaveQuizRequest request, Guid studentId)
    {
        try
        {
            var attempt = await unitOfWork.QuizAttemptRepository.FindOneAsync(
                a => a.Id == attemptId && a.StudentId == studentId);

            if (attempt == null)
                return ApiResponse<bool>.FailureResponse("Không tìm thấy bài làm quiz.");

            if (attempt.Status != QuizAttemptStatus.InProgress)
                return ApiResponse<bool>.FailureResponse("Chỉ có thể lưu bài khi đang làm quiz.");

            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == attempt.QuizId);
            if (quiz?.TimeLimitMinutes.HasValue == true)
            {
                var elapsedMinutes = (DateTime.UtcNow - attempt.StartedAt).TotalMinutes;
                if (elapsedMinutes > quiz.TimeLimitMinutes.Value + 1)
                {
                    attempt.Status = QuizAttemptStatus.Expired;
                    attempt.UpdatedBy = studentId;
                    await unitOfWork.QuizAttemptRepository.UpdateAsync(attemptId, attempt);
                    await unitOfWork.SaveChangesAsync();

                    logger.LogWarning("Quiz attempt {AttemptId} auto-expired due to time limit", attemptId);
                    return ApiResponse<bool>.FailureResponse("Đã hết thời gian làm bài.");
                }
            }

            attempt.Answers = JsonSerializer.Serialize(request.Answers);

            attempt.TimeSpentSeconds = request.TimeSpentSeconds;

            attempt.FlaggedQuestions = JsonSerializer.Serialize(request.FlaggedQuestions);

            await unitOfWork.QuizAttemptRepository.UpdateAsync(attemptId, attempt);
            await unitOfWork.SaveChangesAsync();

            logger.LogDebug(
                "Quiz attempt auto-saved: AttemptId={AttemptId}, AnsweredCount={AnsweredCount}, TimeSpent={TimeSpent}s",
                attemptId, request.Answers.Count, request.TimeSpentSeconds);

            return ApiResponse<bool>.SuccessResponse(true, "Đã lưu bài làm.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error auto-saving quiz attempt: AttemptId={AttemptId}", attemptId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi lưu bài.");
        }
    }

    public async Task<ApiResponse<QuizResultResponse>> GetQuizAttemptResultAsync(Guid attemptId, Guid studentId)
    {
        try
        {
            var (attempt, quiz, errorMessage) = await ValidateAttemptForResultAsync(attemptId, studentId);
            if (attempt == null || quiz == null)
                return ApiResponse<QuizResultResponse>.FailureResponse(errorMessage!);

            var (questionOrder, questionDict) = await LoadQuestionsFromAttemptAsync(attempt);

            var studentAnswers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(attempt.Answers) ?? [];

            var response = attempt.ToResultResponse(quiz, questionOrder, questionDict, studentAnswers);

            return ApiResponse<QuizResultResponse>.SuccessResponse(response, "Lấy kết quả thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting quiz attempt result: AttemptId={AttemptId}", attemptId);
            return ApiResponse<QuizResultResponse>.FailureResponse("Đã xảy ra lỗi khi lấy kết quả.");
        }
    }

    public async Task<ApiResponse<UserQuizAttemptsResponse>> GetUserQuizAttemptsAsync(Guid quizId, Guid studentId)
    {
        try
        {
            var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == quizId);
            if (quiz == null)
                return ApiResponse<UserQuizAttemptsResponse>.FailureResponse("Quiz không tồn tại.");

            var attempts = await unitOfWork.QuizAttemptRepository.GetAllAsync(
                a => a.QuizId == quizId && a.StudentId == studentId);

            var response = attempts.ToUserQuizAttemptsResponse(quiz);

            return ApiResponse<UserQuizAttemptsResponse>.SuccessResponse(response, "Lấy danh sách lượt làm thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user quiz attempts: QuizId={QuizId}, StudentId={StudentId}", quizId, studentId);
            return ApiResponse<UserQuizAttemptsResponse>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách lượt làm.");
        }
    }

    public async Task<ApiResponse<List<Guid>>> FlagQuestionAsync(Guid attemptId, FlagQuestionRequest request, Guid studentId)
    {
        try
        {
            var attempt = await unitOfWork.QuizAttemptRepository.FindOneAsync(a => a.Id == attemptId && a.StudentId == studentId);
            if (attempt == null)
                return ApiResponse<List<Guid>>.FailureResponse("Không tìm thấy bài làm quiz.");

            if (attempt.Status != QuizAttemptStatus.InProgress)
                return ApiResponse<List<Guid>>.FailureResponse("Chỉ có thể đánh dấu câu hỏi khi đang làm bài.");

            var questionOrder = JsonSerializer.Deserialize<List<Guid>>(attempt.QuestionOrder) ?? [];
            if (!questionOrder.Contains(request.QuestionId))
                return ApiResponse<List<Guid>>.FailureResponse("Câu hỏi không thuộc bài quiz này.");

            var flaggedQuestions = JsonSerializer.Deserialize<List<Guid>>(attempt.FlaggedQuestions) ?? [];

            if (request.IsFlagged)
            {
                if (!flaggedQuestions.Contains(request.QuestionId))
                    flaggedQuestions.Add(request.QuestionId);
            }
            else
            {
                flaggedQuestions.Remove(request.QuestionId);
            }

            attempt.FlaggedQuestions = JsonSerializer.Serialize(flaggedQuestions);
            attempt.UpdatedBy = studentId;

            await unitOfWork.QuizAttemptRepository.UpdateAsync(attemptId, attempt);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Question flagged: AttemptId={AttemptId}, QuestionId={QuestionId}, IsFlagged={IsFlagged}",
                attemptId, request.QuestionId, request.IsFlagged);

            return ApiResponse<List<Guid>>.SuccessResponse(flaggedQuestions,
                request.IsFlagged ? "Đã đánh dấu câu hỏi." : "Đã bỏ đánh dấu câu hỏi.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error flagging question: AttemptId={AttemptId}, QuestionId={QuestionId}", attemptId, request.QuestionId);
            return ApiResponse<List<Guid>>.FailureResponse("Đã xảy ra lỗi khi đánh dấu câu hỏi.");
        }
    }


    private static (bool CanStart, string? ErrorMessage) ValidateCanStartQuiz(Quiz quiz, IReadOnlyCollection<QuizAttempt> existingAttempts)
    {
        if (quiz.MaxAttempts > 0 && existingAttempts.Count >= quiz.MaxAttempts)
            return (false, $"Bạn đã hết lượt làm quiz. Tối đa {quiz.MaxAttempts} lượt.");

        var inProgressAttempt = existingAttempts.FirstOrDefault(a => a.Status == QuizAttemptStatus.InProgress);
        if (inProgressAttempt != null)
            return (false, "Bạn đang có một lượt làm quiz chưa hoàn thành. Vui lòng hoàn thành hoặc nộp bài trước.");

        return (true, null);
    }

    private async Task<(QuizAttempt? Attempt, Quiz? Quiz, string? ErrorMessage)> ValidateAttemptForSubmissionAsync(Guid attemptId, Guid studentId)
    {
        var attempt = await unitOfWork.QuizAttemptRepository.FindOneAsync(a => a.Id == attemptId && a.StudentId == studentId);
        if (attempt == null)
            return (null, null, "Không tìm thấy bài làm quiz.");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return (null, null, "Bài làm quiz đã được nộp trước đó.");

        var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == attempt.QuizId);
        if (quiz == null)
            return (null, null, "Quiz không tồn tại.");

        return (attempt, quiz, null);
    }

    private async Task<(QuizAttempt? Attempt, Quiz? Quiz, string? ErrorMessage)> ValidateAttemptForResultAsync(Guid attemptId, Guid studentId)
    {
        var attempt = await unitOfWork.QuizAttemptRepository.FindOneAsync(a => a.Id == attemptId && a.StudentId == studentId);
        if (attempt == null)
            return (null, null, "Không tìm thấy bài làm quiz.");

        if (attempt.Status == QuizAttemptStatus.InProgress)
            return (null, null, "Bài làm quiz chưa được nộp.");

        var quiz = await unitOfWork.QuizRepository.FindOneAsync(q => q.Id == attempt.QuizId);
        if (quiz == null)
            return (null, null, "Quiz không tồn tại.");

        return (attempt, quiz, null);
    }

    private void CheckTimeLimit(Quiz quiz, QuizAttempt attempt, Guid attemptId)
    {
        if (!quiz.TimeLimitMinutes.HasValue) return;

        var elapsedMinutes = (DateTime.UtcNow - attempt.StartedAt).TotalMinutes;
        if (elapsedMinutes > quiz.TimeLimitMinutes.Value + 1)
        {
            logger.LogWarning("Quiz attempt {AttemptId} submitted after time limit", attemptId);
        }
    }

    private async Task<(List<Guid> QuestionOrder, Dictionary<Guid, Question> QuestionDict)> LoadQuestionsFromAttemptAsync(QuizAttempt attempt)
    {
        var questionOrder = JsonSerializer.Deserialize<List<Guid>>(attempt.QuestionOrder) ?? [];
        var questions = await unitOfWork.QuestionRepository.GetAllAsync(q => questionOrder.Contains(q.Id));
        var questionDict = questions.ToDictionary(q => q.Id);
        return (questionOrder, questionDict);
    }

    private async Task UpdateQuizStatisticsAsync(Quiz quiz, bool isPassed)
    {
        quiz.TotalAttempts++;
        if (isPassed) quiz.PassCount++;

        var allAttempts = await unitOfWork.QuizAttemptRepository.GetAllAsync(
            a => a.QuizId == quiz.Id && a.Status == QuizAttemptStatus.Graded);
        quiz.AverageScore = allAttempts.Count > 0
            ? allAttempts.Where(a => a.ScorePercent.HasValue).Average(a => a.ScorePercent!.Value)
            : null;

        await unitOfWork.QuizRepository.UpdateAsync(quiz.Id, quiz);
    }
}
