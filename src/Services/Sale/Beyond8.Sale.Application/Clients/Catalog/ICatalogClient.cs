using Beyond8.Common;
using Beyond8.Common.Clients;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Courses;

namespace Beyond8.Sale.Application.Clients.Catalog;

public interface ICatalogClient : IBaseClient
{
    Task<ApiResponse<CourseDto>> GetCourseByIdAsync(Guid courseId);
}