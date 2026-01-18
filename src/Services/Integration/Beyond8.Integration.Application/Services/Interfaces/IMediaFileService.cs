using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos;

namespace Beyond8.Integration.Application.Services.Interfaces;

public interface IMediaFileService
{
    Task<ApiResponse<UploadFileResponse>> InitiateUploadAsync(Guid userId, UploadFileRequest request, string folder, string? subFolder = null);
    Task<ApiResponse<MediaFileDto>> ConfirmUploadAsync(Guid userId, ConfirmUploadRequest request);
    Task<ApiResponse<MediaFileDto>> GetFileByIdAsync(Guid userId, Guid fileId);
    Task<ApiResponse<bool>> DeleteFileAsync(Guid userId, Guid fileId);
    Task<ApiResponse<List<MediaFileDto>>> GetUserFilesByFolderAsync(Guid userId, string folder);
}
