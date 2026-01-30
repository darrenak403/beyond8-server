using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Assessment.Domain.Repositories.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IQuestionRepository QuestionRepository { get; }
    IQuizRepository QuizRepository { get; }
    IQuizQuestionRepository QuizQuestionRepository { get; }
    IAssignmentRepository AssignmentRepository { get; }
    IQuizAttemptRepository QuizAttemptRepository { get; }
    IAssignmentSubmissionRepository AssignmentSubmissionRepository { get; }
}
