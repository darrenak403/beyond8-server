using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Beyond8.Sale.Infrastructure.ExternalServices;

/// <summary>
/// Low-level VNPay utility for building payment URLs and computing HMAC-SHA512 hashes.
/// VNPay API v2.1.0.
/// </summary>
public class VNPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(StringComparer.Ordinal);

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
            _requestData[key] = value;
    }

    /// <summary>
    /// Builds the VNPay payment URL: base?key=value&amp;...&amp;vnp_SecureHash=HMAC
    /// </summary>
    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var queryBuilder = new StringBuilder();

        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            queryBuilder.Append(WebUtility.UrlEncode(key));
            queryBuilder.Append('=');
            queryBuilder.Append(WebUtility.UrlEncode(value));
            queryBuilder.Append('&');
        }

        var queryString = queryBuilder.ToString().TrimEnd('&');
        var secureHash = ComputeHmacSha512(hashSecret, queryString);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public static string ComputeHmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        return Convert.ToHexStringLower(hashBytes);
    }

    public static string GetResponseDescription(string responseCode) => responseCode switch
    {
        "00" => "Giao dịch thành công",
        "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
        "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
        "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.",
        "11" => "Đã hết hạn chờ thanh toán.",
        "12" => "Thẻ/Tài khoản bị khóa.",
        "13" => "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
        "24" => "Khách hàng hủy giao dịch.",
        "51" => "Tài khoản không đủ số dư.",
        "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày.",
        "75" => "Ngân hàng thanh toán đang bảo trì.",
        "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định.",
        "99" => "Lỗi không xác định.",
        _ => $"Mã lỗi không xác định: {responseCode}"
    };
}
