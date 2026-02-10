namespace Beyond8.Sale.Application.Dtos.Wallets;

/// <summary>
/// Request DTO for instructor wallet top-up via VNPay.
/// </summary>
public class TopUpRequest
{
    /// <summary>
    /// Amount to top up (VND, minimum 10,000)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// VNPay return URL after payment completion
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}
