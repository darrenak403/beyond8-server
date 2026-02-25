using System;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class UpdateOrderSettlementRequest
{
    public DateTime? SettlementEligibleAt { get; set; }

    // Optional note for audit / reason
    public string? Note { get; set; }
}
