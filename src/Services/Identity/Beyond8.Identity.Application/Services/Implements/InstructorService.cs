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

    /// <summary>
    /// Start Helpers
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage, InstructorProfile? Profile)> ValidateProfileForReviewAsync(Guid profileId)
    {
        var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
            p => p.Id == profileId && p.DeletedAt == null);

        if (profile == null)
            return (false, "Đơn đăng ký giảng viên không tồn tại.", null);

        if (profile.VerificationStatus != VerificationStatus.Pending)
            return (false, "Chỉ có thể xử lý đơn đăng ký đang chờ duyệt.", null);

        return (true, null, profile);
    }

    private async Task<ApiResponse<InstructorProfileResponse>> UpdateProfileAndGetResponseAsync(
     InstructorProfile profile, Guid adminId, string successMessage)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        profile.UpdatedBy = adminId;

        await unitOfWork.SaveChangesAsync();

        // Query lại profile với User
        var updatedProfile = await unitOfWork.InstructorProfileRepository.GetByIdAsync(profile.Id);
        if (updatedProfile != null)
        {
            // Load User navigation property
            var user = await unitOfWork.UserRepository.GetByIdAsync(updatedProfile.UserId);
            if (user != null)
            {
                updatedProfile.User = user;
            }
        }

        if (updatedProfile == null || updatedProfile.User == null)
        {
            logger.LogError("Failed to retrieve updated profile {ProfileId} with User", profile.Id);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy thông tin đơn đăng ký.");
        }

        var response = updatedProfile.ToInstructorProfileResponse();
        return ApiResponse<InstructorProfileResponse>.SuccessResponse(response, successMessage);
    }
    /// <summary>
    /// End Helpers
    /// </summary>

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
            };

            instructorProfile.ToCreateInstructorProfileRequest(request);

            await unitOfWork.InstructorProfileRepository.AddAsync(instructorProfile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Instructor application submitted successfully for user {UserId} with profile {ProfileId}",
                userId, instructorProfile.Id);

            // Query lại profile với User
            var savedProfile = await unitOfWork.InstructorProfileRepository.GetByIdAsync(instructorProfile.Id);
            if (savedProfile != null)
            {
                var profileUser = await unitOfWork.UserRepository.GetByIdAsync(savedProfile.UserId);
                if (profileUser != null)
                {
                    savedProfile.User = profileUser;
                }
            }

            if (savedProfile == null || savedProfile.User == null)
            {
                logger.LogError("Failed to retrieve saved instructor profile {ProfileId} for user {UserId}",
                    instructorProfile.Id, userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Đã xảy ra lỗi khi lưu đơn đăng ký.");
            }

            var response = savedProfile.ToInstructorProfileResponse();
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

    public async Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorApplicationAsync(Guid profileId, Guid adminId)
    {
        try
        {
            var (isValid, errorMessage, profile) = await ValidateProfileForReviewAsync(profileId);
            if (!isValid)
            {
                logger.LogWarning("Cannot approve profile {ProfileId}: {Reason}", profileId, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            // 2. Load user
            var user = await unitOfWork.UserRepository.GetByIdAsync(profile!.UserId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for profile {ProfileId}", profile.UserId, profileId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse("Người dùng không tồn tại.");
            }

            // 3. Update profile
            profile.VerificationStatus = VerificationStatus.Verified;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = adminId;

            // 4. Update user role
            if (!user.Roles.Contains(UserRole.Instructor))
            {
                user.Roles.Add(UserRole.Instructor);
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = adminId;
            }

            logger.LogInformation("Approving instructor application for profile {ProfileId} by admin {AdminId}",
                profileId, adminId);

            // 5. Save and return response
            return await UpdateProfileAndGetResponseAsync(
                profile, adminId, "Đơn đăng ký giảng viên đã được duyệt thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving instructor application {ProfileId} by admin {AdminId}",
                profileId, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi duyệt đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> RejectInstructorApplicationAsync(Guid profileId, RejectInstructorApplicationRequest request, Guid adminId)
    {
        try
        {
            // 1. Validate rejection reason
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                logger.LogWarning("Rejection reason is required for profile {ProfileId}", profileId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Lý do từ chối không được để trống.");
            }

            // 2. Validate profile
            var (isValid, errorMessage, profile) = await ValidateProfileForReviewAsync(profileId);
            if (!isValid)
            {
                logger.LogWarning("Cannot reject profile {ProfileId}: {Reason}", profileId, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            // 3. Update profile
            profile!.VerificationStatus = request.VerificationStatus;
            profile.VerificationNotes = request.RejectionReason;

            logger.LogInformation("Rejecting instructor application for profile {ProfileId} by admin {AdminId} with reason: {Reason}",
                profileId, adminId, request.RejectionReason);

            // 4. Save and return response
            return await UpdateProfileAndGetResponseAsync(
                profile, adminId, "Đơn đăng ký giảng viên đã được từ chối.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting instructor application {ProfileId} by admin {AdminId}",
                profileId, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi từ chối đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<List<InstructorProfileResponse>>> GetPendingApplicationsAsync()
    {
        try
        {
            var pendingProfiles = await unitOfWork.InstructorProfileRepository.GetAllAsync(p =>
                p.VerificationStatus == VerificationStatus.Pending);

            if (pendingProfiles == null || !pendingProfiles.Any())
            {
                logger.LogInformation("No pending instructor applications found");
                return ApiResponse<List<InstructorProfileResponse>>.SuccessResponse(
                    new List<InstructorProfileResponse>(),
                    "Không có đơn đăng ký giảng viên nào đang chờ duyệt.");
            }
            var responses = new List<InstructorProfileResponse>();
            foreach (var profile in pendingProfiles)
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    profile.User = user;
                    responses.Add(profile.ToInstructorProfileResponse());
                }
                else
                {
                    logger.LogWarning("User {UserId} not found for pending profile {ProfileId}",
                        profile.UserId, profile.Id);
                }
            }

            logger.LogInformation("Retrieved {Count} pending instructor applications", responses.Count);
            return ApiResponse<List<InstructorProfileResponse>>.SuccessResponse(
                responses,
                "Lấy danh sách đơn đăng ký giảng viên đang chờ duyệt thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving pending instructor applications");
            return ApiResponse<List<InstructorProfileResponse>>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách đơn đăng ký giảng viên đang chờ duyệt.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId)
    {
        try
        {
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.UserId == userId && p.DeletedAt == null);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile not found for user {UserId}", userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Bạn chưa có hồ sơ giảng viên.");
            }

            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for instructor profile {ProfileId}", userId, profile.Id);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Người dùng không tồn tại.");
            }

            profile.User = user;

            var response = profile.ToInstructorProfileResponse();
            return ApiResponse<InstructorProfileResponse>.SuccessResponse(
                response,
                "Lấy hồ sơ giảng viên của bạn thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving instructor profile for user {UserId}", userId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy hồ sơ giảng viên của bạn.");
        }
    }

    public Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid profileId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdForAdminAsync(Guid profileId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<InstructorProfileResponse>>> GetVerifiedInstructorsAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request)
    {
        throw new NotImplementedException();
    }
}
