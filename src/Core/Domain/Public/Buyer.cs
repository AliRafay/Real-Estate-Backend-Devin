using Demo.WebApi.Domain.Common.Contracts;
using Demo.WebApi.Domain.Common.Enums;

namespace Demo.WebApi.Domain.Public;

public class Buyer : AuditableEntity
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
    public BuyerStatus Status { get; set; } = BuyerStatus.Active;
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } // Link to ApplicationUser if needed
}
