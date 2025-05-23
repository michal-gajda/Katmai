namespace Katmai.WebApi;

using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

internal static class BootstrapperExtensions
{
    public static void UseBootstrapper(this WebApplicationBuilder builder, string serviceName, string serviceNamespace)
    {
        builder.Services.AddHealthChecks();

        var serviceVersion = GetServiceVersion();
        var serviceInstanceId = GetServiceInstanceId();

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceNamespace, serviceVersion, autoGenerateServiceInstanceId: false, serviceInstanceId);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.AddOtlpExporter();
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
                .AddHttpClientInstrumentation()
                .AddOtlpExporter());

        builder.Services.AddSingleton(TracerProvider.Default.GetTracer(serviceName, serviceVersion));
    }

    public static void UseBootstrapper(this WebApplication app)
    {
        app.UseHealthChecks("/health");
    }

    private static string GetServiceVersion(string defaultServiceVersion = "")
    {
        return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? defaultServiceVersion;
    }

    private static string GetServiceInstanceId(string defaultServiceInstanceId = "Development")
    {
        return (Environment.GetEnvironmentVariable("BUILD_INFO") ?? defaultServiceInstanceId).ToLowerInvariant();
    }
}
