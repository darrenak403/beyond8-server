using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IInstructorService
{
    Task<ApiResponse<InstructorProfileResponse>> SubmitInstructorProfileAsync(CreateInstructorProfileRequest request, Guid userId);

    Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorProfileAsync(Guid id, Guid adminId);

    Task<ApiResponse<InstructorProfileResponse>> NotApproveInstructorProfileAsync(Guid id, NotApproveInstructorProfileRequest request, Guid adminId);

    Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId);

    Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid id);

    Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request);

    Task<ApiResponse<InstructorProfileAdminResponse>> GetInstructorProfileByIdForAdminAsync(Guid id);

    Task<ApiResponse<List<InstructorProfileAdminResponse>>> GetInstructorProfilesForAdminAsync(PaginationInstructorRequest pagination);
    Task<ApiResponse<bool>> CheckApplyInstructorProfileAsync(Guid userId);
    Task<ApiResponse<bool>> HiddenInstructorProfileAsync(Guid profileId, Guid userId);
    Task<ApiResponse<bool>> UnHiddenInstructorProfileAsync(Guid profileId, Guid userId);

}
