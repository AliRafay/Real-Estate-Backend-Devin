namespace Demo.WebApi.Application.Buyers;

public class CreateBuyerRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public decimal Budget { get; set; }
    public string PreferredLocation { get; set; }
    public int BedroomsRequired { get; set; }
    public int BathroomsRequired { get; set; }
    public bool IsPremium { get; set; }
    public string Notes { get; set; }
    public string UserId { get; set; }
}
