using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Certificates;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface ICertificateService
{
    Task TryIssueCertificateIfEligibleAsync(Guid enrollmentId);
    Task<ApiResponse<CertificateVerificationResponse>> GetByVerificationHashAsync(string hash);
    Task<ApiResponse<List<CertificateSimpleResponse>>> GetMyCertificatesAsync(Guid userId);
    Task<ApiResponse<CertificateDetailResponse>> GetByIdAsync(Guid id, Guid userId);
    Task<ApiResponse<CertificateEligibilityConfigResponse>> GetCertificateEligibilityConfigAsync(Guid courseId, Guid userId);
    Task<ApiResponse<CertificateEligibilityConfigResponse>> UpdateCertificateEligibilityConfigAsync(Guid courseId, UpdateCertificateEligibilityConfigRequest request, Guid userId);
}
