namespace Beyond8.Assessment.Application.Dtos.Reassign;

public class ReassignOverviewResponse
{
    public int TotalPending { get; set; }
    public List<ReassignRequestItemDto> Items { get; set; } = [];
}
