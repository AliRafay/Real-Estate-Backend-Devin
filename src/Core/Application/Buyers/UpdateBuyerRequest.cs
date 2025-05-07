namespace Demo.WebApi.Application.Buyers;

public class UpdateBuyerRequest : CreateBuyerRequest
{
    public Guid Id { get; set; }
}
