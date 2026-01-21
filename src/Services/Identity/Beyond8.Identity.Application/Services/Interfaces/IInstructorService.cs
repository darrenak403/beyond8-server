using System;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;

namespace Beyond8.Identity.Application.Services.Interfaces;

public interface IInstructorService
{
    // LUỒNG 1: Đăng ký giảng viên
    Task<ApiResponse<InstructorProfileResponse>> SubmitInstructorApplicationAsync(
        CreateInstructorProfileRequest request, Guid userId);
    Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorApplicationAsync(
        Guid profileId, Guid adminId);
    Task<ApiResponse<InstructorProfileResponse>> NotApproveInstructorApplicationAsync(
        Guid profileId, NotApproveInstructorApplicationRequest request, Guid adminId);
    Task<ApiResponse<List<InstructorProfileResponse>>> GetPendingApplicationsAsync();

    // Quản lý hồ sơ
    Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId);
    Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid profileId);
    Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdForAdminAsync(Guid profileId);
    Task<ApiResponse<List<InstructorProfileResponse>>> GetVerifiedInstructorsAsync(
        int pageNumber, int pageSize);
    Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(
        Guid userId, UpdateInstructorProfileRequest request);

    // Analytics (Luồng 5)
    // Task<ApiResponse<bool>> UpdateInstructorStatisticsAsync(
    //     Guid instructorId, UpdateInstructorStatisticsRequest request);
    Task<ApiResponse<List<InstructorProfileResponse>>> SearchInstructorsAsync(
        string? searchTerm, List<string>? expertiseAreas, int pageNumber, int pageSize);
    Task<ApiResponse<List<InstructorProfileResponse>>> GetTopInstructorsByRatingAsync(int count);
}
