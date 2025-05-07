using Ardalis.Specification;
using Demo.WebApi.Domain.Public;

namespace Demo.WebApi.Infrastructure.Public;

public class BuyerByEmailSpec : Specification<Buyer>
{
    public BuyerByEmailSpec(string email)
    {
        Query.Where(b => b.Email == email);
    }
}
