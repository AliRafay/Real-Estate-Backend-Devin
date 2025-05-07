using Demo.WebApi.Application.Buyers;
using Demo.WebApi.Application.Common.Specification;
using Demo.WebApi.Domain.Public;

namespace Demo.WebApi.Infrastructure.Public;

public class BuyersBySearchFilterSpec : EntitiesByPaginationFilterSpec<Buyer>
{
    public BuyersBySearchFilterSpec(BuyerListFilter filter, bool countOnly = false)
        : base(filter)
    {
        if (!countOnly)
        {
            Query.OrderByDescending(b => b.RegistrationDate, !filter.HasOrderBy());
        }

        if (!string.IsNullOrEmpty(filter.FirstName))
            Query.Where(b => b.FirstName.Contains(filter.FirstName));

        if (!string.IsNullOrEmpty(filter.LastName))
            Query.Where(b => b.LastName.Contains(filter.LastName));

        if (!string.IsNullOrEmpty(filter.Email))
            Query.Where(b => b.Email.Contains(filter.Email));

        if (!string.IsNullOrEmpty(filter.City))
            Query.Where(b => b.City.Contains(filter.City));

        if (!string.IsNullOrEmpty(filter.State))
            Query.Where(b => b.State.Contains(filter.State));

        if (filter.MinBudget.HasValue)
            Query.Where(b => b.Budget >= filter.MinBudget.Value);

        if (filter.MaxBudget.HasValue)
            Query.Where(b => b.Budget <= filter.MaxBudget.Value);

        if (filter.BedroomsRequired.HasValue)
            Query.Where(b => b.BedroomsRequired == filter.BedroomsRequired.Value);

        if (filter.BathroomsRequired.HasValue)
            Query.Where(b => b.BathroomsRequired == filter.BathroomsRequired.Value);

        if (filter.IsPremium.HasValue)
            Query.Where(b => b.IsPremium == filter.IsPremium.Value);
    }
}
