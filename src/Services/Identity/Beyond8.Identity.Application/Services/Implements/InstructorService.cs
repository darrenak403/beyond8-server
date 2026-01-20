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
    IUnitOfWork unitOfWork
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

            var existingProfile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                var errorResponse = ValidateExistingProfileStatus(existingProfile, userId);
                if (errorResponse != null)
                    return errorResponse;
            }

            logger.LogInformation("Creating instructor profile for user {UserId}", userId);
            var instructorProfile = request.ToInstructorProfileEntity(userId);
            await unitOfWork.InstructorProfileRepository.AddAsync(instructorProfile);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(instructorProfile.ToInstructorProfileResponse(user), "Đã gửi đơn đăng ký giảng viên thành công. Chúng tôi sẽ xem xét và phản hồi sớm nhất.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting instructor application for user {UserId}", userId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse("Đã xảy ra lỗi khi gửi đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorApplicationAsync(Guid profileId, Guid adminId)
    {
        try
        {
            var (isValid, errorMessage, profile, user) = await ValidateProfileForReviewAsync(profileId);
            if (!isValid)
            {
                logger.LogWarning("Cannot approve profile {ProfileId}: {Reason}", profileId, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            logger.LogInformation("Adding instructor role to user {UserId}", user!.Id);
            user!.Roles.Add(UserRole.Instructor);

            logger.LogInformation("Approving instructor application for profile {ProfileId} by admin {AdminId}", profileId, adminId);
            profile!.VerificationStatus = VerificationStatus.Verified;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = adminId;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.InstructorProfileRepository.UpdateAsync(profile.Id, profile);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(profile.ToInstructorProfileResponse(user), "Đơn đăng ký giảng viên đã được duyệt thành công.");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving instructor application {ProfileId} by admin {AdminId}",
                profileId, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi duyệt đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> NotApproveInstructorApplicationAsync(Guid profileId, NotApproveInstructorApplicationRequest request, Guid adminId)
    {
        try
        {
            var (isValid, errorMessage, profile, user) = await ValidateProfileForReviewAsync(profileId);
            if (!isValid)
            {
                logger.LogWarning("Cannot reject profile {ProfileId}: {Reason}", profileId, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            logger.LogInformation("Not approving instructor application for profile {ProfileId} by admin {AdminId} with reason: {Reason}", profileId, adminId, request.NotApproveReason);

            profile!.VerificationStatus = request.VerificationStatus;
            profile.VerificationNotes = request.NotApproveReason;

            await unitOfWork.InstructorProfileRepository.UpdateAsync(profile.Id, profile);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(profile.ToInstructorProfileResponse(user!), "Đơn đăng ký giảng viên đã được không phê duyệt.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error not approving instructor application {ProfileId} by admin {AdminId}",
                profileId, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi không phê duyệt đơn đăng ký giảng viên.");
        }
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

    public Task<ApiResponse<List<InstructorProfileResponse>>> SearchInstructorsAsync(string? searchTerm, List<string>? expertiseAreas, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request)
    {
        throw new NotImplementedException();
    }

    private ApiResponse<InstructorProfileResponse>? ValidateExistingProfileStatus(InstructorProfile existingProfile, Guid userId)
    {
        logger.LogWarning("User {UserId} already has an instructor profile with status {Status}",
            userId, existingProfile.VerificationStatus);

        return existingProfile.VerificationStatus switch
        {
            VerificationStatus.Pending =>
                ApiResponse<InstructorProfileResponse>.FailureResponse("Bạn đã có đơn đăng ký giảng viên đang chờ duyệt."),
            VerificationStatus.Verified =>
                ApiResponse<InstructorProfileResponse>.FailureResponse("Bạn đã là giảng viên được xác minh."),
            VerificationStatus.RequestUpdate =>
                ApiResponse<InstructorProfileResponse>.FailureResponse("Đơn đăng ký giảng viên đang được yêu cầu cập nhật. Vui lòng cập nhật hồ sơ của bạn."),
            VerificationStatus.Rejected =>
                ApiResponse<InstructorProfileResponse>.FailureResponse("Đơn đăng ký của bạn đã bị từ chối. Vui lòng liên hệ hỗ trợ để biết thêm chi tiết."),
            _ =>
                ApiResponse<InstructorProfileResponse>.FailureResponse("Trạng thái đơn không hợp lệ.")
        };
    }

    private async Task<(bool IsValid, string? ErrorMessage, InstructorProfile? Profile, User? User)> ValidateProfileForReviewAsync(Guid profileId)
    {
        var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.Id == profileId);

        if (profile == null)
            return (false, "Đơn đăng ký giảng viên không tồn tại.", null, null);

        if (profile.VerificationStatus != VerificationStatus.Pending)
            return (false, "Chỉ có thể xử lý đơn đăng ký đang chờ duyệt.", null, null);

        var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
        if (user == null)
            return (false, "Người dùng không tồn tại.", null, null);

        return (true, null, profile, user);
    }
}
