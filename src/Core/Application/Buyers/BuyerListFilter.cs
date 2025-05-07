using Demo.WebApi.Application.Common.Models;

namespace Demo.WebApi.Application.Buyers;

public class BuyerListFilter : PaginationFilter
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public int? BedroomsRequired { get; set; }
    public int? BathroomsRequired { get; set; }
    public bool? IsPremium { get; set; }
}
