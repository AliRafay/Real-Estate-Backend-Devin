namespace Demo.WebApi.Infrastructure.Payment;
public class StripeSettings
{
    public string? ApiKey { get; set; }
    public string? PlatformAccountId { get; set;}
    public string? AccountWebhookSigningSecret { get; set; }
    public string? ConnectWebhookSigningSecret { get; set; }
}
