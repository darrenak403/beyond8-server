using Beyond8.Catalog.Application.Dtos.CourseDocuments;
using Beyond8.Catalog.Application.Mappings.CourseDocumentMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class CourseDocumentService(
    ILogger<CourseDocumentService> logger,
    IUnitOfWork unitOfWork) : ICourseDocumentService
{
    public async Task<ApiResponse<List<CourseDocumentResponse>>> GetCourseDocumentsAsync(Guid courseId, Guid currentUserId)
    {
        try
        {
            // Verify course ownership
            var course = await unitOfWork.CourseRepository.FindOneAsync(c =>
                c.Id == courseId && c.IsActive && c.InstructorId == currentUserId);

            if (course == null)
            {
                logger.LogWarning("Course not found or access denied: {CourseId} for user: {UserId}", courseId, currentUserId);
                return ApiResponse<List<CourseDocumentResponse>>.FailureResponse("Khóa học không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var documents = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Where(d => d.CourseId == courseId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToResponse())
                .ToListAsync();

            return ApiResponse<List<CourseDocumentResponse>>.SuccessResponse(documents, "Lấy danh sách tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course documents for course: {CourseId}", courseId);
            return ApiResponse<List<CourseDocumentResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài liệu.");
        }
    }

    public async Task<ApiResponse<CourseDocumentResponse>> GetCourseDocumentByIdAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Course.IsActive && d.Course.InstructorId == currentUserId);

            if (document == null)
            {
                logger.LogWarning("Document not found or access denied: {DocumentId} for user: {UserId}", documentId, currentUserId);
                return ApiResponse<CourseDocumentResponse>.FailureResponse("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
            }

            return ApiResponse<CourseDocumentResponse>.SuccessResponse(document.ToResponse(), "Lấy thông tin tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course document: {DocumentId}", documentId);
            return ApiResponse<CourseDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin tài liệu.");
        }
    }

    public async Task<ApiResponse<CourseDocumentResponse>> CreateCourseDocumentAsync(CreateCourseDocumentRequest request, Guid currentUserId)
    {
        try
        {
            // Verify course ownership
            var course = await unitOfWork.CourseRepository.FindOneAsync(c =>
                c.Id == request.CourseId && c.IsActive && c.InstructorId == currentUserId);

            if (course == null)
            {
                logger.LogWarning("Course not found or access denied: {CourseId} for user: {UserId}", request.CourseId, currentUserId);
                return ApiResponse<CourseDocumentResponse>.FailureResponse("Khóa học không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var document = new CourseDocument
            {
                CourseId = request.CourseId,
                Title = request.Title,
                Description = request.Description,
                CourseDocumentUrl = request.CourseDocumentUrl,
                IsDownloadable = request.IsDownloadable
            };

            await unitOfWork.CourseDocumentRepository.AddAsync(document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course document created: {DocumentId} for course: {CourseId}", document.Id, request.CourseId);
            return ApiResponse<CourseDocumentResponse>.SuccessResponse(document.ToResponse(), "Tạo tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course document for course: {CourseId}", request.CourseId);
            return ApiResponse<CourseDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi tạo tài liệu.");
        }
    }

    public async Task<ApiResponse<CourseDocumentResponse>> UpdateCourseDocumentAsync(Guid documentId, UpdateCourseDocumentRequest request, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Course.IsActive && d.Course.InstructorId == currentUserId);

            if (document == null)
            {
                logger.LogWarning("Document not found or access denied: {DocumentId} for user: {UserId}", documentId, currentUserId);
                return ApiResponse<CourseDocumentResponse>.FailureResponse("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
            }

            document.Title = request.Title;
            document.Description = request.Description;
            document.CourseDocumentUrl = request.CourseDocumentUrl;
            document.IsDownloadable = request.IsDownloadable;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CourseDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course document updated: {DocumentId}", documentId);
            return ApiResponse<CourseDocumentResponse>.SuccessResponse(document.ToResponse(), "Cập nhật tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course document: {DocumentId}", documentId);
            return ApiResponse<CourseDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật tài liệu.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCourseDocumentAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Course.IsActive && d.Course.InstructorId == currentUserId);

            if (document == null)
            {
                logger.LogWarning("Document not found or access denied: {DocumentId} for user: {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
            }

            await unitOfWork.CourseDocumentRepository.DeleteAsync(documentId);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course document deleted: {DocumentId}", documentId);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course document: {DocumentId}", documentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa tài liệu.");
        }
    }

    public async Task<ApiResponse<bool>> ToggleDownloadableAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Course.IsActive && d.Course.InstructorId == currentUserId);

            if (document == null)
            {
                logger.LogWarning("Document not found or access denied: {DocumentId} for user: {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
            }

            document.IsDownloadable = !document.IsDownloadable;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CourseDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course document downloadable toggled: {DocumentId} to {IsDownloadable}", documentId, document.IsDownloadable);
            return ApiResponse<bool>.SuccessResponse(true, $"Tài liệu đã được {(document.IsDownloadable ? "cho phép" : "không cho phép")} tải xuống.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling downloadable status for document: {DocumentId}", documentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi thay đổi trạng thái tải xuống.");
        }
    }

    public async Task<ApiResponse<bool>> IncrementDownloadCountAsync(Guid documentId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository.FindOneAsync(d => d.Id == documentId);
            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại.");
            }

            document.DownloadCount++;
            await unitOfWork.CourseDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Download count incremented for document: {DocumentId}", documentId);
            return ApiResponse<bool>.SuccessResponse(true, "Đã tăng số lượt tải xuống.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error incrementing download count for document: {DocumentId}", documentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi tăng số lượt tải xuống.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateVectorIndexStatusAsync(Guid documentId, bool isIndexed, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.CourseDocumentRepository
                .AsQueryable()
                .Include(d => d.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.Course.IsActive && d.Course.InstructorId == currentUserId);

            if (document == null)
            {
                logger.LogWarning("Document not found or access denied: {DocumentId} for user: {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
            }

            document.IsIndexedInVectorDb = isIndexed;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CourseDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Vector index status updated for document: {DocumentId} to {IsIndexed}", documentId, isIndexed);
            return ApiResponse<bool>.SuccessResponse(true, $"Trạng thái vector index đã được cập nhật thành {(isIndexed ? "đã index" : "chưa index")}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vector index status for document: {DocumentId}", documentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật trạng thái vector index.");
        }
    }
}