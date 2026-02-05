using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Certificates;

namespace Beyond8.Learning.Application.Services.Interfaces;

public interface ICertificateService
{
    Task TryIssueCertificateIfEligibleAsync(Guid enrollmentId);
    Task<ApiResponse<CertificateVerificationResponse>> GetByVerificationHashAsync(string hash);
}
