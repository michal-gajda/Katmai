namespace Katmai.WebApi;

using Katmai.Application;
using Katmai.Infrastructure;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public sealed class Program
{
    private const int EXIT_SUCCESS = 0;
    private const string INSTANCE_ID = "development";

    private const string SERVICE_NAME = "katmai";
    private const string SERVICE_NAMESPACE = "otel";
    private const string SERVICE_VERSION = "1.0.0";

    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHealthChecks();

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(SERVICE_NAME, SERVICE_NAMESPACE, SERVICE_VERSION, autoGenerateServiceInstanceId: false, INSTANCE_ID);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder).AddOtlpExporter();
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter());

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseHealthChecks("/health");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();

        return EXIT_SUCCESS;
    }
}
