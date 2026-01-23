using Beyond8.Common.Events.Identity;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Mappings;
using Beyond8.Identity.Application.Services.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Beyond8.Identity.Application.Services.Implements;

public class InstructorService(
    ILogger<InstructorService> logger,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : IInstructorService
{
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

    private async Task<(bool IsSuccess, string? ErrorMessage, InstructorProfile? Profile, User? User)> GetProfileWithUserAsync(Guid profileId)
    {
        var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.Id == profileId && p.VerificationStatus == VerificationStatus.Verified);
        if (profile == null)
            return (false, "Hồ sơ giảng viên không tồn tại hoặc chưa được duyệt.", null, null);

        return (true, null, profile, profile.User);
    }

    public async Task<ApiResponse<InstructorProfileResponse>> SubmitInstructorProfileAsync(CreateInstructorProfileRequest request, Guid userId)
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
                p => p.UserId == userId && p.VerificationStatus != VerificationStatus.Hidden);

            if (existingProfile != null)
            {
                logger.LogWarning("User {UserId} already has an instructor application with status {Status}",
                    userId, existingProfile.VerificationStatus);

                if (existingProfile.VerificationStatus == VerificationStatus.Pending)
                {
                    return ApiResponse<InstructorProfileResponse>.FailureResponse("Bạn đã có đơn đăng ký giảng viên đang chờ duyệt.");
                }
                else if (existingProfile.VerificationStatus == VerificationStatus.Verified)
                {
                    return ApiResponse<InstructorProfileResponse>.FailureResponse("Bạn đã là giảng viên được xác minh.");
                }
                else if (existingProfile.VerificationStatus == VerificationStatus.RequestUpdate)
                {
                    return ApiResponse<InstructorProfileResponse>.FailureResponse("Đơn đăng ký giảng viên đang được yêu cầu cập nhật. Vui lòng cập nhật hồ sơ của bạn.");
                }
            }

            var instructorProfile = request.ToInstructorProfileEntity(userId);

            await unitOfWork.InstructorProfileRepository.AddAsync(instructorProfile);
            await unitOfWork.SaveChangesAsync();

            var submittedEvent = new InstructorApplicationSubmittedEvent(
                user.Id,
                instructorProfile.Id,
                user.FullName,
                user.Email,
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(submittedEvent);

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(instructorProfile.ToInstructorProfileResponse(user), "Đã gửi đơn đăng ký giảng viên thành công. Chúng tôi sẽ xem xét và phản hồi sớm nhất.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting instructor application for user {UserId}", userId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse("Đã xảy ra lỗi khi gửi đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> ApproveInstructorProfileAsync(Guid id, Guid adminId)
    {
        try
        {
            var (isValid, errorMessage, profile, user) = await ValidateProfileForReviewAsync(id);
            if (!isValid)
            {
                logger.LogWarning("Cannot approve profile {ProfileId}: {Reason}", id, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            logger.LogInformation("Adding instructor role to user {UserId}", user!.Id);
            user!.Roles.Add(UserRole.Instructor);

            logger.LogInformation("Approving instructor application for profile {ProfileId} by admin {AdminId}", id, adminId);
            profile!.VerificationStatus = VerificationStatus.Verified;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = adminId;

            await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            await unitOfWork.InstructorProfileRepository.UpdateAsync(id, profile);
            await unitOfWork.SaveChangesAsync();

            // Publish event for approval email
            var frontendUrl = configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
            var profileUrl = $"{frontendUrl}/instructor/me";
            var approvalEvent = new InstructorApprovalEmailEvent(
                user.Id,
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
                id, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi duyệt đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> NotApproveInstructorProfileAsync(Guid id, NotApproveInstructorProfileRequest request, Guid adminId)
    {
        try
        {
            var (isValid, errorMessage, profile, user) = await ValidateProfileForReviewAsync(id);
            if (!isValid)
            {
                logger.LogWarning("Cannot not approve profile {ProfileId}: {Reason}", id, errorMessage);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(errorMessage!);
            }

            logger.LogInformation("Not approving instructor application for profile {ProfileId} by admin {AdminId} with reason: {Reason}", id, adminId, request.NotApproveReason);

            profile!.VerificationStatus = request.VerificationStatus;
            profile.VerificationNotes = request.NotApproveReason;

            await unitOfWork.InstructorProfileRepository.UpdateAsync(id, profile);
            await unitOfWork.SaveChangesAsync();

            var updateRequestEvent = new InstructorUpdateRequestEmailEvent(
                user!.Id,
                user.Email,
                user.FullName,
                request.NotApproveReason,
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(updateRequestEvent);

            return ApiResponse<InstructorProfileResponse>.SuccessResponse(profile.ToInstructorProfileResponse(user!), "Đơn đăng ký giảng viên đã được không phê duyệt.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error not approving instructor application {ProfileId} by admin {AdminId}",
                id, adminId);
            return ApiResponse<InstructorProfileResponse>.FailureResponse(
                "Đã xảy ra lỗi khi không phê duyệt đơn đăng ký giảng viên.");
        }
    }

    public async Task<ApiResponse<List<InstructorProfileAdminResponse>>> GetInstructorProfilesForAdminAsync(PaginationInstructorRequest pagination)
    {
        try
        {
            var profile = await unitOfWork.InstructorProfileRepository.SearchInstructorsPagedAsync(
                pagination.PageNumber,
                pagination.PageSize,
                pagination.Email,
                pagination.FullName,
                pagination.PhoneNumber,
                pagination.Bio,
                pagination.HeadLine,
                pagination.ExpertiseAreas,
                pagination.SchoolName,
                pagination.CompanyName,
                pagination.IsDescending.HasValue ? pagination.IsDescending.Value : true);

            var profileResponses = profile.Items
                .Select(p => p.ToInstructorProfileAdminResponse(p.User!))
                .ToList();

            logger.LogInformation("Retrieved {Count} instructor profiles for admin", profileResponses.Count);

            return ApiResponse<List<InstructorProfileAdminResponse>>.SuccessPagedResponse(
                profileResponses,
                profile.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách hồ sơ giảng viên thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving instructor profiles");
            return ApiResponse<List<InstructorProfileAdminResponse>>.FailureResponse(
                "Đã xảy ra lỗi khi lấy danh sách hồ sơ giảng viên.");
        }
    }

    public async Task<ApiResponse<InstructorProfileResponse>> GetMyInstructorProfileAsync(Guid userId)
    {
        try
        {
            // Exclude Hidden profiles - user should not see deleted profiles
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.UserId == userId);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile not found for user {UserId}", userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Hồ sơ giảng viên của bạn không tồn tại.");
            }

            var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for instructor profile {ProfileId}", userId, profile.Id);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Người dùng không tồn tại.");
            }

            var response = profile.ToInstructorProfileResponse(user!);
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
            // Exclude Hidden profiles - user cannot update deleted profiles
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.UserId == userId && p.VerificationStatus != VerificationStatus.Hidden);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile not found for user {UserId}", userId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Hồ sơ giảng viên của bạn không tồn tại.");
            }

            if (profile.VerificationStatus != VerificationStatus.Verified && profile.VerificationStatus != VerificationStatus.RequestUpdate)
            {
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Bạn chỉ có thể cập nhật hồ sơ giảng viên khi đã được duyệt hoặc đang được yêu cầu cập nhật.");
            }

            profile.ToUpdateInstructorProfileRequest(request);

            await unitOfWork.InstructorProfileRepository.UpdateAsync(profile.Id, profile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated instructor profile for user {UserId}", userId);

            var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for instructor profile {ProfileId}", userId, profile.Id);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Người dùng không tồn tại.");
            }

            var updateRequestEvent = new InstructorUpdateRequestEmailEvent(
                user!.Id,
                user.Email,
                user.FullName,
                "Đơn đăng ký giảng viên đang được yêu cầu cập nhật. Vui lòng cập nhật hồ sơ của bạn.",
                DateTime.UtcNow
            );
            await publishEndpoint.Publish(updateRequestEvent);

            var response = profile.ToInstructorProfileResponse(user!);
            return ApiResponse<InstructorProfileResponse>.SuccessResponse(
                response,
                "Cập nhật hồ sơ giảng viên thành công. Vui lòng chờ duyệt lại hồ sơ của bạn.");

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
            // Public endpoint - only show verified profiles, exclude Hidden
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.Id == profileId &&
                     p.VerificationStatus == VerificationStatus.Verified);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile {ProfileId} not found or not verified", profileId);
                return ApiResponse<InstructorProfileResponse>.FailureResponse(
                    "Hồ sơ giảng viên không tồn tại hoặc chưa được duyệt.");
            }

            var user = profile.User;

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
            // Admin can view all profiles including Hidden (for reference)
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.Id == profileId);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile {ProfileId} not found for admin", profileId);
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

    public async Task<ApiResponse<bool>> CheckApplyInstructorProfileAsync(Guid userId)
    {
        try
        {
            // Exclude Hidden profiles - treated as not applied
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(
                p => p.UserId == userId && p.VerificationStatus != VerificationStatus.Hidden);

            if (profile == null)
            {
                return ApiResponse<bool>.SuccessResponse(false, "Bạn chưa gửi đơn đăng ký giảng viên.");
            }
            return ApiResponse<bool>.SuccessResponse(true, "Bạn đã gửi đơn đăng ký giảng viên thành công. Chúng tôi sẽ xem xét và phản hồi sớm nhất.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user {UserId} has applied for instructor profile", userId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi kiểm tra xem bạn đã gửi đơn đăng ký giảng viên chưa.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteInstructorProfileAsync(Guid profileId, Guid adminId)
    {
        try
        {
            var profile = await unitOfWork.InstructorProfileRepository.FindOneAsync(p => p.Id == profileId && p.VerificationStatus != VerificationStatus.Hidden);

            if (profile == null)
            {
                logger.LogWarning("Instructor profile {ProfileId} not found for deletion by admin {AdminId}",
                    profileId, adminId);
                return ApiResponse<bool>.FailureResponse(
                    "Hồ sơ giảng viên không tồn tại.");
            }

            if (profile.VerificationStatus == VerificationStatus.Hidden)
            {
                logger.LogWarning("Instructor profile {ProfileId} already hidden, cannot delete again", profileId);
                return ApiResponse<bool>.FailureResponse(
                    "Hồ sơ giảng viên đã bị xóa trước đó.");
            }

            var user = await unitOfWork.UserRepository.GetByIdAsync(profile.UserId);
            if (user == null)
            {
                logger.LogError("User {UserId} not found for instructor profile {ProfileId}",
                    profile.UserId, profileId);
                return ApiResponse<bool>.FailureResponse("Người dùng không tồn tại.");
            }

            logger.LogInformation("Deleting instructor profile {ProfileId} by admin {AdminId}", profileId, adminId);

            profile.VerificationStatus = VerificationStatus.Hidden;

            if (user.Roles.Contains(UserRole.Instructor))
            {
                logger.LogInformation("Removing instructor role from user {UserId}", user.Id);
                user.Roles.Remove(UserRole.Instructor);
                await unitOfWork.UserRepository.UpdateAsync(user.Id, user);
            }

            await unitOfWork.InstructorProfileRepository.UpdateAsync(profileId, profile);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully deleted instructor profile {ProfileId} by admin {AdminId}", profileId, adminId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa hồ sơ giảng viên thành công.");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting instructor profile {ProfileId} by admin {AdminId}",
                profileId, adminId);
            return ApiResponse<bool>.FailureResponse(
                "Đã xảy ra lỗi khi xóa hồ sơ giảng viên.");
        }
    }
}
