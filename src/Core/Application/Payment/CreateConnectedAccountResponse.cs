using Newtonsoft.Json;

namespace Demo.WebApi.Application.Payment;
public class CreateConnectedAccountResponse
{
    [JsonProperty("object")]
    public string? Object { get; set; }

    [JsonProperty("created")]
    public DateTime Created { get; set; }

    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }
}
