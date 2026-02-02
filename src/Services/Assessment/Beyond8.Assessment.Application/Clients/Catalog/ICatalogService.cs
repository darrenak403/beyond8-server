using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Clients.Catalog
{
    public interface ICatalogService : IBaseClient
    {
        Task<ApiResponse<bool>> UpdateQuizForLessonAsync(Guid lessonId, Guid? quizId);
    }
}