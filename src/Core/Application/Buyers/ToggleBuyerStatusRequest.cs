namespace Demo.WebApi.Application.Buyers;

public class ToggleBuyerStatusRequest
{
    public Guid BuyerId { get; set; }
    public bool Block { get; set; }
}
