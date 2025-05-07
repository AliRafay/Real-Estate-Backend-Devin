using Demo.WebApi.Application.Buyers;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.WebApi.Infrastructure.Public;

public static class Startup
{
    public static IServiceCollection AddPublicInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IBuyerService, BuyerService>();
        
        return services;
    }
}
