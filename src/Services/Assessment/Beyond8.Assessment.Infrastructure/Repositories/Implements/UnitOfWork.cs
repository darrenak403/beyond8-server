using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Assessment.Infrastructure.Repositories.Implements;

public class UnitOfWork(AssessmentDbContext context) : BaseUnitOfWork<AssessmentDbContext>(context), IUnitOfWork
{
    private IQuestionRepository? _questionRepository;
    private IQuizRepository? _quizRepository;
    private IQuizQuestionRepository? _quizQuestionRepository;
    private IAssignmentRepository? _assignmentRepository;
    private IQuizAttemptRepository? _quizAttemptRepository;
    private IAssignmentSubmissionRepository? _assignmentSubmissionRepository;

    public IQuestionRepository QuestionRepository => _questionRepository ??= new QuestionRepository(context);
    public IQuizRepository QuizRepository => _quizRepository ??= new QuizRepository(context);
    public IQuizQuestionRepository QuizQuestionRepository => _quizQuestionRepository ??= new QuizQuestionRepository(context);
    public IAssignmentRepository AssignmentRepository => _assignmentRepository ??= new AssignmentRepository(context);
    public IQuizAttemptRepository QuizAttemptRepository => _quizAttemptRepository ??= new QuizAttemptRepository(context);
    public IAssignmentSubmissionRepository AssignmentSubmissionRepository => _assignmentSubmissionRepository ??= new AssignmentSubmissionRepository(context);
}
