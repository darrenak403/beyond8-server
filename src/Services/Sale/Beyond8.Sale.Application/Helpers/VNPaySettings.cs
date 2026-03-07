namespace Beyond8.Sale.Application.Helpers;

/// <summary>
/// VNPay gateway configuration. Bound from appsettings.json "VNPay" section.
/// </summary>
public class VNPaySettings
{
    public const string SectionName = "VNPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>Frontend URL to redirect user after payment processing</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>API Gateway URL for VNPay to call back (e.g. http://localhost:8080)</summary>
    public string BackendCallbackUrl { get; set; } = string.Empty;

    /// <summary>Path appended to auto-detected frontend origin (e.g. /payment/callback)</summary>
    public string CallbackPath { get; set; } = "/payment/callback";

    /// <summary>Whitelist of allowed frontend origins. Empty = allow any.</summary>
    public List<string> AllowedOrigins { get; set; } = [];

    public string Version { get; set; } = "2.1.0";
    public int PaymentExpiryMinutes { get; set; } = 15;
}
