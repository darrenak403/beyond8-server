using Beyond8.Sale.Application.Dtos.VNPays;

namespace Beyond8.Sale.Application.Helpers;

public interface IVNPayService
{
    string CreatePaymentUrl(VNPayPaymentInfo paymentInfo, string clientIpAddress);
    bool ValidateCallback(string rawQueryString, out VNPayCallbackResult result);
    string ResolveReturnUrl(string? origin, string? referer);
}
