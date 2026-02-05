namespace Beyond8.Sale.Application.Dtos.Payments;

public class PayOSCallbackRequest
{
    public string code { get; set; } = string.Empty;
    public string desc { get; set; } = string.Empty;
    public bool success { get; set; }
    public PayOSData? data { get; set; }
    public string signature { get; set; } = string.Empty;
}

public class PayOSData
{
    public string orderCode { get; set; } = string.Empty;
    public decimal amount { get; set; }
    public string description { get; set; } = string.Empty;
    public string accountNumber { get; set; } = string.Empty;
    public string reference { get; set; } = string.Empty;
    public string transactionDateTime { get; set; } = string.Empty;
    public string currency { get; set; } = "VND";
    public string paymentLinkId { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
}
