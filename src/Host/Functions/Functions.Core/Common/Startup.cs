using Demo.WebApi.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace Functions.Infrastructure.Common;

internal static class Startup
{
    internal static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped);
}
