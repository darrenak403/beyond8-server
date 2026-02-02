using Beyond8.Assessment.Domain.Entities;

namespace Beyond8.Assessment.Application.Mappings.QuizQuestionMappings
{
    public static class QuizQuestionMappings
    {
        public static QuizQuestion ToQuizQuestionEntity(Guid quizId, Guid questionId, int orderIndex)
        {
            return new QuizQuestion
            {
                QuizId = quizId,
                QuestionId = questionId,
                OrderIndex = orderIndex
            };
        }
    }
}