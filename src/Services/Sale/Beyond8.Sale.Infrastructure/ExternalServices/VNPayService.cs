using System.Globalization;
using Beyond8.Sale.Application.Dtos.VNPays;
using Beyond8.Sale.Application.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beyond8.Sale.Infrastructure.ExternalServices;

public class VNPayService(
    IOptions<VNPaySettings> options,
    ILogger<VNPayService> logger) : IVNPayService
{
    private readonly VNPaySettings _config = options.Value;

    public string CreatePaymentUrl(VNPayPaymentInfo paymentInfo, string clientIpAddress)
    {
        var vnpay = new VNPayLibrary();

        vnpay.AddRequestData("vnp_Version", _config.Version);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", _config.TmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(paymentInfo.Amount * 100)).ToString());
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_TxnRef", paymentInfo.PaymentNumber);
        vnpay.AddRequestData("vnp_OrderInfo", paymentInfo.OrderInfo);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_Locale", paymentInfo.Locale ?? "vn");
        vnpay.AddRequestData("vnp_ReturnUrl", paymentInfo.ReturnUrl ?? _config.ReturnUrl);
        vnpay.AddRequestData("vnp_IpAddr", clientIpAddress);
        vnpay.AddRequestData("vnp_CreateDate", paymentInfo.CreatedDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_ExpireDate", paymentInfo.ExpireDate.ToString("yyyyMMddHHmmss"));

        if (!string.IsNullOrEmpty(paymentInfo.BankCode))
            vnpay.AddRequestData("vnp_BankCode", paymentInfo.BankCode);

        var paymentUrl = vnpay.CreateRequestUrl(_config.BaseUrl, _config.HashSecret);

        logger.LogInformation("VNPay URL generated — PaymentNumber: {PaymentNumber}, Amount: {Amount}",
            paymentInfo.PaymentNumber, paymentInfo.Amount);

        return paymentUrl;
    }

    /// <summary>
    /// Validates VNPay callback by computing HMAC-SHA512 from the raw HTTP query string.
    /// Raw query string is used because ASP.NET auto-decodes query values (e.g. "Thanh+toan" → "Thanh toan"),
    /// which produces a different hash than what VNPay computed on the encoded values.
    /// </summary>
    public bool ValidateCallback(string rawQueryString, out VNPayCallbackResult result)
    {
        var sortedParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var decodedParams = new Dictionary<string, string>();
        var secureHash = string.Empty;

        foreach (var part in rawQueryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eqIdx = part.IndexOf('=');
            if (eqIdx <= 0) continue;

            var key = part[..eqIdx];
            var rawValue = part[(eqIdx + 1)..];

            if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
            {
                secureHash = rawValue;
                continue;
            }
            if (key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                continue;

            sortedParams[key] = rawValue;
            decodedParams[key] = Uri.UnescapeDataString(rawValue.Replace('+', ' '));
        }

        // Rebuild hash data from sorted raw (encoded) params — matches VNPay's computation
        var hashData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={kv.Value}"));
        var computedHash = VNPayLibrary.ComputeHmacSha512(_config.HashSecret, hashData);
        var isValid = string.Equals(computedHash, secureHash, StringComparison.OrdinalIgnoreCase);

        string GetDecoded(string key) => decodedParams.GetValueOrDefault(key, string.Empty);

        var responseCode = GetDecoded("vnp_ResponseCode");
        var transactionStatus = GetDecoded("vnp_TransactionStatus");

        decimal.TryParse(GetDecoded("vnp_Amount"), CultureInfo.InvariantCulture, out var amountRaw);

        result = new VNPayCallbackResult
        {
            IsValidSignature = isValid,
            IsSuccess = isValid && responseCode == "00" && transactionStatus == "00",
            TxnRef = GetDecoded("vnp_TxnRef"),
            TransactionNo = GetDecoded("vnp_TransactionNo"),
            ResponseCode = responseCode,
            TransactionStatus = transactionStatus,
            Amount = amountRaw / 100, // VNPay sends amount × 100
            BankCode = GetDecoded("vnp_BankCode"),
            CardType = GetDecoded("vnp_CardType"),
            PayDate = GetDecoded("vnp_PayDate"),
            OrderInfo = GetDecoded("vnp_OrderInfo"),
            ResponseDescription = VNPayLibrary.GetResponseDescription(responseCode)
        };

        logger.LogInformation(
            "VNPay callback — TxnRef: {TxnRef}, Code: {Code}, Valid: {Valid}, Success: {Success}",
            result.TxnRef, result.ResponseCode, isValid, result.IsSuccess);

        return isValid;
    }

    /// <summary>
    /// Resolves the frontend return URL from request Origin/Referer headers.
    /// Falls back to configured ReturnUrl if origin is missing or not in whitelist.
    /// </summary>
    public string ResolveReturnUrl(string? origin, string? referer)
    {
        var resolvedOrigin = origin;

        if (string.IsNullOrEmpty(resolvedOrigin)
            && !string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            resolvedOrigin = $"{refererUri.Scheme}://{refererUri.Authority}";
        }

        if (!string.IsNullOrEmpty(resolvedOrigin) && _config.AllowedOrigins.Count > 0)
        {
            var isAllowed = _config.AllowedOrigins
                .Any(a => string.Equals(a.TrimEnd('/'), resolvedOrigin.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                logger.LogWarning("Origin {Origin} not in whitelist, using fallback", resolvedOrigin);
                resolvedOrigin = null;
            }
        }

        return !string.IsNullOrEmpty(resolvedOrigin)
            ? $"{resolvedOrigin.TrimEnd('/')}{_config.CallbackPath}"
            : _config.ReturnUrl;
    }
}
