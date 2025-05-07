using Functions.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationFunction.Settings;
using Shared.Services.AzureStorage;

IConfiguration? configuration = null;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddOptions<FCMSettings>()
            .BindConfiguration(nameof(FCMSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MailSettings>()
            .BindConfiguration(nameof(MailSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MessageSettings>()
            .BindConfiguration(nameof(MessageSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddInfrastructure(configuration);
        services.AddAzureQueues();
    })
    .Build();

host.Run();
