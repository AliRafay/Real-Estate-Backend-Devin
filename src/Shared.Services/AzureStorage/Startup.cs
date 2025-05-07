using Microsoft.Extensions.DependencyInjection;
using Demo.WebApi.Application.Storage;

namespace Shared.Services.AzureStorage;

public static class Startup
{
    public static IServiceCollection AddAzureQueues(this IServiceCollection services) =>
        services.AddService(typeof(IStorageQueueClient<>), typeof(AzureQueueClient<>), ServiceLifetime.Scoped);
}
