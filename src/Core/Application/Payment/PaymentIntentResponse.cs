namespace Demo.WebApi.Application.Payment;
public class PaymentIntentResponse
{
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Status { get; set;}
    public string? LatestChargeId { get; set; }
    public decimal AmountCharged { get; set; }
    public Dictionary<string, string>? MetaData { get; set; }
}
