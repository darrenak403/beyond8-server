using Beyond8.Common.Events.Identity;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Mappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class InstructorService(
    ILogger<InstructorService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint
) : IInstructorService
{

    /// <summary>
    /// Start Helpers
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage, InstructorProfile? Profile, User? User)> ValidateProfileForReviewAsync(Guid profileId)
    {
        var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
            p => p.Id == profileId && p.DeletedAt == null);

        if (profile == null)
            return (false, "Đơn đăng ký giảng viên không tồn tại.", null, null);

        if (profile.VerificationStatus != VerificationStatus.Pending)
            return (false, "Chỉ có thể xử lý đơn đăng ký đang chờ duyệt.", null, null);

        var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
        if (user == null)
            return (false, "Người dùng không tồn tại.", null, null);

        return (true, null, profile, user);
    }

    private async Task<(bool IsSuccess, string? ErrorMessage, InstructorProfile? Profile, User? User)>
    GetProfileWithUserAsync(Guid profileId)
    {
        var profile = await unitOfWork.InstructorProfileRepository.GetByIdAsync(profileId);
        if (profile == null)
            return (false, "Hồ sơ giảng viên không tồn tại.", null, null);

        var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
        if (user == null)
            return (false, "Người dùng không tồn tại.", null, null);

        profile.User = user;
        return (true, null, profile, user);
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

            var existingProfile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.UserId == userId);
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

            var instructorProfile = request.ToInstructorProfileEntity(userId);
            instructorProfile.CreatedBy = userId;
            instructorProfile.CreatedAt = DateTime.UtcNow;

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

            // Publish event for approval email
            var profileUrl = $"https://beyond8.dev/instructor/{user.Id}"; // TODO: Update with actual profile URL
            var approvalEvent = new InstructorApprovalEmailEvent(
                user.Email,
                user.FullName,
                profileUrl,
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(approvalEvent);

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

            // Publish appropriate event based on verification status
            if (profile.VerificationStatus == VerificationStatus.Rejected)
            {
                var rejectionEvent = new InstructorRejectionEmailEvent(
                    user!.Email,
                    user.FullName,
                    request.NotApproveReason,
                    DateTime.UtcNow
                );
                await publishEndpoint.Publish(rejectionEvent);
            }
            else if (profile.VerificationStatus == VerificationStatus.RequestUpdate)
            {
                var updateRequestEvent = new InstructorUpdateRequestEmailEvent(
                    user!.Email,
                    user.FullName,
                    request.NotApproveReason,
                    DateTime.UtcNow
                );
                await publishEndpoint.Publish(updateRequestEvent);
            }

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
                    responses.Add(profile.ToInstructorProfileResponse(user));
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
                    "Hồ sơ giảng viên của bạn không tồn tại.");
            }

            var user = await unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for instructor profile {ProfileId}", userId, profile.Id);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Người dùng không tồn tại.");
            }

            profile.User = user;

            var response = profile.ToInstructorProfileResponse(user);
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

    public async Task<ApiResponse<InstructorProfileResponse>> UpdateInstructorProfileAsync(Guid userId, UpdateInstructorProfileRequest request)
    {
        try
        {
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.UserId == userId);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile not found for user {UserId}", userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Hồ sơ giảng viên của bạn không tồn tại.");
            }
            profile.ToUpdateInstructorProfileRequest(request);

            await unitOfWork.InstructorProfileRepository.UpdateAsync(profile.Id, profile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated instructor profile for user {UserId}", userId);

            var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
            profile.User = user!;

            var response = profile.ToInstructorProfileResponse(user!);
            return ApiResponse<InstructorProfileResponse>.SuccessResponse(
                response,
                "Cập nhật hồ sơ giảng viên thành công.");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating instructor profile for user {UserId}", userId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi cập nhật hồ sơ giảng viên.");
        }
    }
    public async Task<ApiResponse<InstructorProfileResponse>> GetInstructorProfileByIdAsync(Guid profileId)
    {
        try
        {
            var (isSuccess, errorMessage, profile, user) = await GetProfileWithUserAsync(profileId);
            if (!isSuccess)
            {
                logger.LogWarning("Instructor profile {ProfileId} not found", profileId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            var response = profile!.ToInstructorProfileResponse(user!);
            return ApiResponse<InstructorProfileResponse>.SuccessResponse(
                response,
                "Lấy hồ sơ giảng viên thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving instructor profile {ProfileId}", profileId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy hồ sơ giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileAdminResponse>> GetInstructorProfileByIdForAdminAsync(Guid profileId)
    {
        try
        {
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.Id == profileId && p.DeletedAt == null);

            if (profile == null)
            {
                logger.LogInformation("Instructor profile {ProfileId} not found for admin", profileId);
                return ApiResponse<InstructorProfileAdminResponse>.FailureResponse(
                    "Hồ sơ giảng viên không tồn tại.");
            }

            var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
            profile.User = user!;

            var response = profile.ToInstructorProfileAdminResponse(user!);
            return ApiResponse<InstructorProfileAdminResponse>.SuccessResponse(
                response,
                "Lấy hồ sơ giảng viên thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving instructor profile {ProfileId} for admin", profileId);
            return ApiResponse<InstructorProfileAdminResponse>.FailureResponse(
                "Đã xảy ra lỗi khi lấy hồ sơ giảng viên cho quản trị viên.");
        }
    }

    public async Task<ApiResponse<List<InstructorProfileResponse>>> GetVerifiedInstructorsAsync(int pageNumber, int pageSize)
    {
        try
        {
            var verifiedProfiles = await unitOfWork.InstructorProfileRepository.GetAllAsync(p =>
                p.VerificationStatus == VerificationStatus.Verified);

            if (verifiedProfiles == null || !verifiedProfiles.Any())
            {
                logger.LogInformation("No verified instructors found");
                return ApiResponse<List<InstructorProfileResponse>>.SuccessResponse(
                    new List<InstructorProfileResponse>(),
                    "Không có giảng viên đã được xác minh.");
            }

            var pagedProfiles = verifiedProfiles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var responses = new List<InstructorProfileResponse>();
            foreach (var profile in pagedProfiles)
            {
                var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    profile.User = user;
                    responses.Add(profile.ToInstructorProfileResponse(user));
                }
                else
                {
                    logger.LogWarning("User {UserId} not found for verified profile {ProfileId}",
                        profile.UserId, profile.Id);
                }
            }

            logger.LogInformation("Retrieved {Count} verified instructors for page {PageNumber}", responses.Count, pageNumber);
            return ApiResponse<List<InstructorProfileResponse>>.SuccessPagedResponse(
                    responses,
                    verifiedProfiles.Count,
                    pageNumber,
                    pageSize,
                    "Lấy danh sách giảng viên được xác minh thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving verified instructors");
            return ApiResponse<List<InstructorProfileResponse>>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách giảng viên đã được xác minh.");
        }
    }
}
