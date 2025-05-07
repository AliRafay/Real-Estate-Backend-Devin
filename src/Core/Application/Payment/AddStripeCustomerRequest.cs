namespace Demo.WebApi.Application.Payment;
public class AddStripeCustomerRequest
{
    public string Name { get; set; } = default!;

    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? Phone { get; set; }
}