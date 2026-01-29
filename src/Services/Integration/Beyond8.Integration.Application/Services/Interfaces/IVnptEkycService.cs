using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.VnptEkyc;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IVnptEkycService
    {
        Task<UploadResponse> UploadAsync(IFormFile file);
        Task<LivenessResponse> CheckLivenessAsync(LivenessRequest request);
        Task<ApiResponse<LivenessResponse>> UploadAndCheckLivenessAsync(IFormFile file);
        Task<ApiResponse<ClassifyWithOcrResponse>> ClassifyAsync(ClassifyRequest request);
    }
}
