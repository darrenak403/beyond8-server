using Beyond8.Catalog.Application.Dtos.Sections;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Services.Interfaces;

public interface ISectionService
{
    Task<ApiResponse<List<SectionResponse>>> GetSectionsByCourseIdAsync(Guid courseId, Guid currentUserId);
    Task<ApiResponse<SectionResponse>> GetSectionByIdAsync(Guid sectionId, Guid currentUserId);
    Task<ApiResponse<SectionResponse>> CreateSectionAsync(CreateSectionRequest request, Guid currentUserId);
    Task<ApiResponse<SectionResponse>> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request, Guid currentUserId);
    Task<ApiResponse<bool>> DeleteSectionAsync(Guid sectionId, Guid currentUserId);
    Task<ApiResponse<bool>> UpdateSectionAssignmentAsync(Guid sectionId, Guid? assignmentId, Guid currentUserId);
}
