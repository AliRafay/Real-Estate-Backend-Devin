namespace Demo.WebApi.Application.Buyers;

public class ToggleBuyerStatusRequest
{
    public int BuyerId { get; set; }
    public bool Block { get; set; }
}
