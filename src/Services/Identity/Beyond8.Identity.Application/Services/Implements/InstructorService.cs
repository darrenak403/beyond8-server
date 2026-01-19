using System;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Mappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class InstructorService(
    ILogger<InstructorService> logger,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IInstructorService
{
    public async Task<ApiResponse<InstructorProfileResponse>> SubmitInstructorApplicationAsync(CreateInstructorProfileRequest request, Guid userId)
    {
        try
        {
            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User with ID {UserId} not found when submitting instructor application", userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse("Người dùng không tồn tại.");
            }

            var existingProfile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.UserId == userId && p.DeletedAt == null
            );

            if (existingProfile != null)
            {
                logger.LogWarning("User {UserId} already has an instructor application with status {Status}",
                    userId, existingProfile.VerificationStatus);

                return existingProfile.VerificationStatus switch
                {
                    VerificationStatus.Pending => ApiResponse<InstructorProfileResponse>.FailureResponse(
                        "Bạn đã có đơn đăng ký giảng viên đang chờ duyệt."),
                    VerificationStatus.Verified => ApiResponse<InstructorProfileResponse>.FailureResponse(
                        "Bạn đã là giảng viên được xác minh."),
                    VerificationStatus.Rejected => ApiResponse<InstructorProfileResponse>.FailureResponse(
                        "Đơn đăng ký của bạn đã bị từ chối. Vui lòng liên hệ admin để biết thêm chi tiết."),
                    _ => ApiResponse<InstructorProfileResponse>.FailureResponse(
                        "Bạn đã có đơn đăng ký giảng viên.")
                };
            }

            var instructorProfile = new InstructorProfile
            {
                UserId = userId,
                VerificationStatus = VerificationStatus.Pending,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            instructorProfile.ToCreateInstructorProfileRequest(request);

            await unitOfWork.InstructorProfileRepository.AddAsync(instructorProfile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Instructor application submitted successfully for user {UserId} with profile {ProfileId}",
                userId, instructorProfile.Id);

            // 7. Map sang response và trả về
            var response = instructorProfile.ToInstructorProfileResponse();

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(
                response,
                "Đơn đăng ký giảng viên đã được gửi thành công. Chúng tôi sẽ xem xét và phản hồi sớm nhất."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting instructor application for user {UserId}", userId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse("Đã xảy ra lỗi khi gửi đơn đăng ký giảng viên.");
        }
    }

    public Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorApplicationAsync(Guid profileId, Guid adminId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid profileId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<InstructorProfileResponse>>> GetPendingApplicationsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<InstructorProfileResponse>>> GetTopInstructorsByRatingAsync(int count)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<InstructorProfileResponse>>> GetVerifiedInstructorsAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> RejectInstructorApplicationAsync(Guid profileId, string rejectionReason, Guid adminId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<InstructorProfileResponse>>> SearchInstructorsAsync(string? searchTerm, List<string>? expertiseAreas, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> UpdateInstructorStatisticsAsync(Guid instructorId, UpdateInstructorStatisticsRequest request)
    {
        throw new NotImplementedException();
    }
}
