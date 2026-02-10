using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using Beyond8.Catalog.Application.Mappings.LessonDocumentMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class LessonDocumentService(
    ILogger<LessonDocumentService> logger,
    IUnitOfWork unitOfWork) : ILessonDocumentService
{
    public async Task<ApiResponse<List<LessonDocumentResponse>>> GetLessonDocumentsAsync(Guid lessonId, Guid currentUserId)
    {
        try
        {
            // Verify lesson ownership through course
            var validationResult = await CheckLessonOwnershipAsync(lessonId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<List<LessonDocumentResponse>>.FailureResponse(validationResult.ErrorMessage!);

            var documents = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Where(d => d.LessonId == lessonId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToResponse())
                .ToListAsync();

            return ApiResponse<List<LessonDocumentResponse>>.SuccessResponse(documents, "Lấy danh sách tài liệu bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson documents for lesson: {LessonId}", lessonId);
            return ApiResponse<List<LessonDocumentResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài liệu bài học.");
        }
    }

    public async Task<ApiResponse<List<LessonDocumentResponse>>> GetLessonDocumentsForStudentAsync(Guid lessonId, Guid currentUserId)
    {
        try
        {
            var documents = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Where(d => d.LessonId == lessonId && d.IsDownloadable)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.ToResponse())
                .ToListAsync();

            return ApiResponse<List<LessonDocumentResponse>>.SuccessResponse(documents, "Lấy danh sách tài liệu bài học cho học viên thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson documents for student for lesson: {LessonId}", lessonId);
            return ApiResponse<List<LessonDocumentResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài liệu bài học cho học viên.");
        }
    }

    public async Task<ApiResponse<LessonDocumentResponse>> GetLessonDocumentByIdAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Include(d => d.Lesson)
                .ThenInclude(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<LessonDocumentResponse>.FailureResponse("Tài liệu không tồn tại.");
            }

            // Verify ownership through course
            if (document.Lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for document {DocumentId} by user {UserId}", documentId, currentUserId);
                return ApiResponse<LessonDocumentResponse>.FailureResponse("Bạn không có quyền truy cập tài liệu này.");
            }

            return ApiResponse<LessonDocumentResponse>.SuccessResponse(document.ToResponse(), "Lấy thông tin tài liệu thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson document: {DocumentId}", documentId);
            return ApiResponse<LessonDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin tài liệu.");
        }
    }

    public async Task<ApiResponse<LessonDocumentResponse>> CreateLessonDocumentAsync(CreateLessonDocumentRequest request, Guid currentUserId)
    {
        try
        {
            // Verify lesson ownership through course
            var validationResult = await CheckLessonOwnershipAsync(request.LessonId, currentUserId);
            if (!validationResult.IsValid)
                return ApiResponse<LessonDocumentResponse>.FailureResponse(validationResult.ErrorMessage!);

            var document = request.ToEntity();
            await unitOfWork.LessonDocumentRepository.AddAsync(document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lesson document created: {DocumentId} for lesson {LessonId} by user {UserId}",
                document.Id, request.LessonId, currentUserId);

            return ApiResponse<LessonDocumentResponse>.SuccessResponse(document.ToResponse(), "Tạo tài liệu bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lesson document for lesson: {LessonId}", request.LessonId);
            return ApiResponse<LessonDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi tạo tài liệu bài học.");
        }
    }

    public async Task<ApiResponse<LessonDocumentResponse>> UpdateLessonDocumentAsync(Guid documentId, UpdateLessonDocumentRequest request, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Include(d => d.Lesson)
                .ThenInclude(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<LessonDocumentResponse>.FailureResponse("Tài liệu không tồn tại.");
            }

            // Verify ownership through course
            if (document.Lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for document {DocumentId} by user {UserId}", documentId, currentUserId);
                return ApiResponse<LessonDocumentResponse>.FailureResponse("Bạn không có quyền truy cập tài liệu này.");
            }

            document.UpdateFrom(request);
            await unitOfWork.LessonDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lesson document updated: {DocumentId} by user {UserId}", documentId, currentUserId);

            return ApiResponse<LessonDocumentResponse>.SuccessResponse(document.ToResponse(), "Cập nhật tài liệu bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lesson document: {DocumentId}", documentId);
            return ApiResponse<LessonDocumentResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật tài liệu bài học.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteLessonDocumentAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Include(d => d.Lesson)
                .ThenInclude(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại.");
            }

            // Verify ownership through course
            if (document.Lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for document {DocumentId} by user {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền truy cập tài liệu này.");
            }

            await unitOfWork.LessonDocumentRepository.DeleteAsync(documentId);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lesson document deleted: {DocumentId} by user {UserId}", documentId, currentUserId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa tài liệu bài học thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lesson document: {DocumentId}", documentId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa tài liệu bài học.");
        }
    }

    public async Task<ApiResponse<bool>> ToggleDownloadableAsync(Guid documentId, Guid currentUserId)
    {
        try
        {
            var document = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Include(d => d.Lesson)
                .ThenInclude(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại.");
            }

            // Verify ownership through course
            if (document.Lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for document {DocumentId} by user {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền truy cập tài liệu này.");
            }

            document.IsDownloadable = !document.IsDownloadable;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.LessonDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Lesson document downloadable toggled: {DocumentId} to {IsDownloadable}", documentId, document.IsDownloadable);
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
            var document = await unitOfWork.LessonDocumentRepository.FindOneAsync(d => d.Id == documentId);
            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại.");
            }

            document.DownloadCount++;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.LessonDocumentRepository.UpdateAsync(documentId, document);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Download count incremented for document: {DocumentId}", documentId);
            return ApiResponse<bool>.SuccessResponse(true, "Tăng số lượt tải xuống thành công.");
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
            var document = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Include(d => d.Lesson)
                .ThenInclude(l => l.Section)
                .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                logger.LogWarning("Document not found: {DocumentId}", documentId);
                return ApiResponse<bool>.FailureResponse("Tài liệu không tồn tại.");
            }

            // Verify ownership through course
            if (document.Lesson.Section.Course.InstructorId != currentUserId)
            {
                logger.LogWarning("Access denied for document {DocumentId} by user {UserId}", documentId, currentUserId);
                return ApiResponse<bool>.FailureResponse("Bạn không có quyền truy cập tài liệu này.");
            }

            document.IsIndexedInVectorDb = isIndexed;
            document.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.LessonDocumentRepository.UpdateAsync(documentId, document);
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

    private async Task<(bool IsValid, string? ErrorMessage)> CheckLessonOwnershipAsync(Guid lessonId, Guid currentUserId)
    {
        var lesson = await unitOfWork.LessonRepository.AsQueryable()
            .Include(l => l.Section)
            .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
        {
            logger.LogWarning("Lesson not found: {LessonId}", lessonId);
            return (false, "Bài học không tồn tại.");
        }

        if (lesson.Section.Course.InstructorId != currentUserId)
        {
            logger.LogWarning("Access denied for lesson {LessonId} by user {UserId}", lessonId, currentUserId);
            return (false, "Bạn không có quyền truy cập bài học này.");
        }

        return (true, null);
    }

    public async Task<ApiResponse<List<LessonDocumentResponse>>> GetLessonDocumentsPreviewAsync(Guid lessonId)
    {
        try
        {
            var lesson = await unitOfWork.LessonRepository.FindOneAsync(l => l.Id == lessonId && l.IsPreview == true);
            if (lesson == null)
            {
                logger.LogWarning("Lesson not found for preview: {LessonId}", lessonId);
                return ApiResponse<List<LessonDocumentResponse>>.FailureResponse("Bài học không tồn tại.");
            }

            // Get only downloadable documents for preview (limit to 10 for performance)
            var documents = await unitOfWork.LessonDocumentRepository
                .AsQueryable()
                .Where(d => d.LessonId == lessonId && d.IsDownloadable)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10) // Limit preview documents
                .Select(d => d.ToResponse())
                .ToListAsync();

            logger.LogInformation("Retrieved {Count} preview documents for lesson {LessonId}", documents.Count, lessonId);
            return ApiResponse<List<LessonDocumentResponse>>.SuccessResponse(documents, "Lấy danh sách tài liệu preview thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lesson documents preview for lesson {LessonId}", lessonId);
            return ApiResponse<List<LessonDocumentResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách tài liệu preview.");
        }
    }
}