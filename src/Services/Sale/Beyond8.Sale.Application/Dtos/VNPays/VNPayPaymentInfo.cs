namespace Beyond8.Sale.Application.Dtos.VNPays;

public class VNPayPaymentInfo
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string? Locale { get; set; } = "vn";
    public string? ReturnUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpireDate { get; set; }
}