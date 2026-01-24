using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.MediaFiles;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IMediaFileService
{
    Task<ApiResponse<UploadFileResponse>> InitiateUploadAsync(Guid userId, UploadFileRequest request, string folder, string? subFolder = null);
    Task<ApiResponse<MediaFileDto>> ConfirmUploadAsync(Guid userId, ConfirmUploadRequest request);
    Task<ApiResponse<MediaFileDto>> GetFileByIdAsync(Guid userId, Guid fileId);
    Task<ApiResponse<bool>> DeleteFileAsync(Guid userId, Guid fileId);
    Task<ApiResponse<List<MediaFileDto>>> GetUserFilesByFolderAsync(Guid userId, string folder);

    // New methods for Course Catalog service
    Task<ApiResponse<MediaFileInfoDto>> GetFileInfoByCloudFrontUrlAsync(string cloudFrontUrl);
    Task<ApiResponse<DownloadUrlDto>> GetDownloadUrlAsync(string cloudFrontUrl, bool inline = false);
}
