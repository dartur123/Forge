namespace Forge.Application.Requests;

public class PostSubconOrderRequest
{
    public string OrderNumber { get; set; }
    public int SubcontractorId { get; set; }
    public string Currency { get; set; } = "PHP";
    public decimal ExchangeRate { get; set; } = 1.0m;

    public List<PostSubconOrderLineRequest>? Lines { get; set; }
}