using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.MediaFiles;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements
{
    public class MediaFileService(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ILogger<MediaFileService> logger) : IMediaFileService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<MediaFileService> _logger = logger;

        public async Task<ApiResponse<UploadFileResponse>> InitiateUploadAsync(
            Guid userId,
            UploadFileRequest request,
            string folder,
            string? subFolder = null)
        {
            try
            {
                var extension = Path.GetExtension(request.FileName);
                var fileKey = _storageService.BuildFileKey(folder, userId, request.FileName, subFolder);

                var mediaFile = new MediaFile
                {
                    UserId = userId,
                    Provider = StorageProvider.S3,
                    FilePath = fileKey,
                    OriginalFileName = request.FileName,
                    ContentType = request.ContentType,
                    Extension = extension,
                    Size = request.Size,
                    Status = FileStatus.Pending,
                    Metadata = request.Metadata
                };

                await _unitOfWork.MediaFileRepository.AddAsync(mediaFile);
                await _unitOfWork.SaveChangesAsync();

                var presignedUrl = _storageService.GeneratePresignedUploadUrl(fileKey, request.ContentType);

                var response = new UploadFileResponse
                {
                    FileId = mediaFile.Id,
                    PresignedUrl = presignedUrl,
                    FileKey = fileKey,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    FileName = request.FileName,
                    ContentType = request.ContentType
                };

                _logger.LogInformation("Initiated upload for user {UserId}, file {FileName}", userId, request.FileName);

                return ApiResponse<UploadFileResponse>.SuccessResponse(
                    response,
                    "Presigned URL được tạo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating upload for user {UserId}", userId);
                return ApiResponse<UploadFileResponse>.FailureResponse("Không thể khởi tạo upload file");
            }
        }

        public async Task<ApiResponse<MediaFileDto>> ConfirmUploadAsync(Guid userId, ConfirmUploadRequest request)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(
                    f => f.Id == request.FileId && f.UserId == userId);

                if (mediaFile == null)
                {
                    return ApiResponse<MediaFileDto>.FailureResponse("File không tồn tại");
                }

                if (mediaFile.Status == FileStatus.Uploaded)
                {
                    return ApiResponse<MediaFileDto>.FailureResponse("File đã được xác nhận trước đó");
                }

                var fileExists = await _storageService.FileExistsAsync(mediaFile.FilePath);
                if (!fileExists)
                {
                    mediaFile.Status = FileStatus.Failed;
                    await _unitOfWork.SaveChangesAsync();
                    return ApiResponse<MediaFileDto>.FailureResponse("File chưa được upload lên S3");
                }

                mediaFile.Status = FileStatus.Uploaded;
                await _unitOfWork.MediaFileRepository.UpdateAsync(mediaFile.Id, mediaFile);
                await _unitOfWork.SaveChangesAsync();

                var dto = MapToDto(mediaFile);

                _logger.LogInformation("Confirmed upload for file {FileId}", request.FileId);

                return ApiResponse<MediaFileDto>.SuccessResponse(dto, "File được xác nhận thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming upload for file {FileId}", request.FileId);
                return ApiResponse<MediaFileDto>.FailureResponse("Không thể xác nhận file upload");
            }
        }

        public async Task<ApiResponse<MediaFileDto>> GetFileByIdAsync(Guid userId, Guid fileId)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(
                    f => f.Id == fileId && f.UserId == userId);

                if (mediaFile == null)
                {
                    return ApiResponse<MediaFileDto>.FailureResponse("File không tồn tại");
                }

                var dto = MapToDto(mediaFile);

                return ApiResponse<MediaFileDto>.SuccessResponse(dto, "Lấy thông tin file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileId}", fileId);
                return ApiResponse<MediaFileDto>.FailureResponse("Không thể lấy thông tin file");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileAsync(Guid userId, Guid fileId)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(
                    f => f.Id == fileId && f.UserId == userId);

                if (mediaFile == null)
                {
                    return ApiResponse<bool>.FailureResponse("File không tồn tại");
                }

                await _storageService.DeleteFileAsync(mediaFile.FilePath);

                mediaFile.Status = FileStatus.Deleted;
                await _unitOfWork.MediaFileRepository.UpdateAsync(mediaFile.Id, mediaFile);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted file {FileId}", fileId);

                return ApiResponse<bool>.SuccessResponse(true, "Xóa file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return ApiResponse<bool>.FailureResponse("Không thể xóa file");
            }
        }

        public async Task<ApiResponse<List<MediaFileDto>>> GetUserFilesByFolderAsync(Guid userId, string folder)
        {
            try
            {
                var files = await _unitOfWork.MediaFileRepository.GetAllAsync(
                    f => f.UserId == userId && f.FilePath.StartsWith(folder) && f.Status == FileStatus.Uploaded);

                var dtos = files.Select(MapToDto).ToList();

                return ApiResponse<List<MediaFileDto>>.SuccessResponse(dtos, "Lấy danh sách file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files for user {UserId} in folder {Folder}", userId, folder);
                return ApiResponse<List<MediaFileDto>>.FailureResponse("Không thể lấy danh sách file");
            }
        }

        public async Task<ApiResponse<MediaFileInfoDto>> GetFileInfoByCloudFrontUrlAsync(string cloudFrontUrl)
        {
            try
            {
                var s3Key = _storageService.ExtractKeyFromUrl(cloudFrontUrl);
                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(
                    f => f.FilePath == s3Key && f.Status == FileStatus.Uploaded);

                if (mediaFile == null)
                {
                    return ApiResponse<MediaFileInfoDto>.FailureResponse(
                        "File không tồn tại với CloudFront URL được cung cấp");
                }

                var dto = new MediaFileInfoDto
                {
                    FileId = mediaFile.Id,
                    UserId = mediaFile.UserId,
                    FileName = mediaFile.OriginalFileName,
                    FileExtension = mediaFile.Extension?.TrimStart('.') ?? string.Empty,
                    ContentType = mediaFile.ContentType,
                    Size = mediaFile.Size,
                    SizeFormatted = FormatFileSize(mediaFile.Size),
                    Folder = ExtractFolder(mediaFile.FilePath),
                    SubFolder = ExtractSubFolder(mediaFile.FilePath),
                    S3Key = mediaFile.FilePath,
                    CloudFrontUrl = _storageService.GetFilePath(mediaFile.FilePath),
                    Status = mediaFile.Status.ToString(),
                    UploadedAt = mediaFile.UpdatedAt,
                    CreatedAt = mediaFile.CreatedAt
                };

                return ApiResponse<MediaFileInfoDto>.SuccessResponse(
                    dto,
                    "Lấy thông tin file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info by CloudFront URL: {Url}", cloudFrontUrl);
                return ApiResponse<MediaFileInfoDto>.FailureResponse(
                    "Không thể lấy thông tin file");
            }
        }

        public async Task<ApiResponse<DownloadUrlDto>> GetDownloadUrlAsync(
            string cloudFrontUrl,
            bool inline = false)
        {
            try
            {
                var s3Key = _storageService.ExtractKeyFromUrl(cloudFrontUrl);
                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(
                    f => f.FilePath == s3Key && f.Status == FileStatus.Uploaded);

                if (mediaFile == null)
                {
                    return ApiResponse<DownloadUrlDto>.FailureResponse("File không tồn tại");
                }

                var disposition = inline ? "inline" : "attachment";
                var presignedUrl = _storageService.GeneratePresignedDownloadUrl(
                    mediaFile.FilePath,
                    mediaFile.OriginalFileName,
                    disposition,
                    expirationMinutes: 15);

                var expiresAt = DateTime.UtcNow.AddMinutes(15);

                var dto = new DownloadUrlDto
                {
                    DownloadUrl = presignedUrl,
                    FileName = mediaFile.OriginalFileName,
                    ExpiresAt = expiresAt,
                    ExpiresIn = "15 minutes"
                };

                return ApiResponse<DownloadUrlDto>.SuccessResponse(
                    dto,
                    "Tạo URL download thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating download URL for: {Url}", cloudFrontUrl);
                return ApiResponse<DownloadUrlDto>.FailureResponse(
                    "Không thể tạo URL download");
            }
        }

        private MediaFileDto MapToDto(MediaFile mediaFile)
        {
            return new MediaFileDto
            {
                Id = mediaFile.Id,
                UserId = mediaFile.UserId,
                Provider = mediaFile.Provider,
                FilePath = mediaFile.FilePath,
                FileUrl = _storageService.GetFilePath(mediaFile.FilePath),
                OriginalFileName = mediaFile.OriginalFileName,
                ContentType = mediaFile.ContentType,
                Extension = mediaFile.Extension,
                Size = mediaFile.Size,
                Status = mediaFile.Status,
                Metadata = mediaFile.Metadata,
                CreatedAt = mediaFile.CreatedAt
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string ExtractFolder(string filePath)
        {
            var parts = filePath.Split('/');
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private static string? ExtractSubFolder(string filePath)
        {
            var parts = filePath.Split('/');
            return parts.Length > 2 ? parts[2] : null;
        }
    }
}
