namespace Beyond8.Sale.Application.Dtos.VNPays;

public class VNPayCallbackResult
{
    public bool IsValidSignature { get; set; }
    public bool IsSuccess { get; set; }
    public string TxnRef { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string PayDate { get; set; } = string.Empty;
    public string OrderInfo { get; set; } = string.Empty;
    public string ResponseDescription { get; set; } = string.Empty;
}
