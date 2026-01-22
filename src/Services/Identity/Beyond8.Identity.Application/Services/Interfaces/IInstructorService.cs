using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IInstructorService
{
    Task<ApiResponse<InstructorProfileResponse>> SubmitInstructorProfileAsync(CreateInstructorProfileRequest request, Guid userId);

    Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorProfileAsync(Guid id, Guid adminId);

    Task<ApiResponse<InstructorProfileResponse>> NotApproveInstructorProfileAsync(Guid id, NotApproveInstructorProfileRequest request, Guid adminId);

    Task<ApiResponse<List<InstructorProfileAdminResponse>>> GetInstructorProfilesAsync(PaginationInstructorRequest pagination);

    Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId);

    Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid id);

    Task<ApiResponse<InstructorProfileAdminResponse>> GetInstructorProfileByIdForAdminAsync(Guid id);

    Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request);
}
