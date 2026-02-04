using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Catalog;

namespace Beyond8.Learning.Application.Clients.Catalog;

public interface ICatalogClient : IBaseClient
{
    Task<ApiResponse<CourseStructureResponse>> GetCourseStructureAsync(Guid courseId);
}
