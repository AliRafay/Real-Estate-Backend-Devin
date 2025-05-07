using Demo.WebApi.Application;
using Demo.WebApi.Application.Common.Enums;
using Demo.WebApi.Host.Configurations;
using Demo.WebApi.Host.Controllers;
using Demo.WebApi.Infrastructure;
using Demo.WebApi.Infrastructure.Common;
using Demo.WebApi.Infrastructure.Common.Convertor;
using Demo.WebApi.Infrastructure.Logging.Serilog;
using FluentValidation.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text.Json.Serialization;

[assembly: ApiConventionType(typeof(ApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations().RegisterSerilog();
    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = new Dictionary<string, string>();
                foreach (var pair in context.ModelState)
                {
                    if (pair.Value.Errors.Count > 0)
                    {
                        errors.Add(pair.Key, string.Join(Environment.NewLine, pair.Value.Errors.Select(error => error.ErrorMessage).ToList()));
                    }
                }

                return new BadRequestObjectResult(new HttpResponseDto<object>
                {
                    Metadata =
                    new HttpResponseMetadata
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Type = HttpResponseType.Error.ToString(),
                        Message = "Invalid Request",
                        ValidationErrors = errors.Select(x => new Dictionary<string, string> { { x.Key, x.Value } }).ToArray()
                    }
                });
            };
        });

    builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

    builder.Services.AddInfrastructure(builder.Configuration);
    var app = builder.Build();

    await app.Services.InitializeDatabasesAsync();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}