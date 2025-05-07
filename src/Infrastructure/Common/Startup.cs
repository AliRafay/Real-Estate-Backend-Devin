using Demo.WebApi.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Shared.Services;

namespace Demo.WebApi.Infrastructure.Common;

internal static class Startup
{
    internal static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped)
            .AddServices(typeof(ISingletonService), ServiceLifetime.Singleton);
}