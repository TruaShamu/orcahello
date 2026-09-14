using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace NotificationSystem.ServiceDefaults;

/// <summary>
/// Shared "service defaults" for OrcaHello .NET services: a single call that turns on
/// OpenTelemetry logging, metrics and tracing exported over OTLP (picked up by the Aspire
/// dashboard, and by any real collector in a deployed environment).
///
/// The canonical Aspire ServiceDefaults template extends <c>IHostApplicationBuilder</c>
/// (used by ASP.NET Core / minimal-host apps). The NotificationSystem Functions app is an
/// isolated worker built with the classic <see cref="IHostBuilder"/>
/// (<c>new HostBuilder().ConfigureFunctionsWorkerDefaults()</c>), so the defaults are
/// exposed as an <see cref="IHostBuilder"/> extension here instead.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds OpenTelemetry logging, metrics and tracing with OTLP export. The OTLP endpoint
    /// and headers are read from the standard <c>OTEL_EXPORTER_OTLP_*</c> environment
    /// variables, which the Aspire app host injects automatically into each resource it
    /// starts — so no configuration is needed for local runs.
    /// </summary>
    public static IHostBuilder AddServiceDefaults(this IHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.AddOpenTelemetry(o =>
            {
                o.IncludeFormattedMessage = true;
                o.IncludeScopes = true;
            });
        });

        builder.ConfigureServices(services => services.ConfigureOpenTelemetry());

        return builder;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation());

        // Honors OTEL_EXPORTER_OTLP_ENDPOINT (injected by the Aspire app host) for all
        // three signals; a no-op when the variable is unset (e.g. non-Aspire runs).
        services.AddOpenTelemetry().UseOtlpExporter();

        return services;
    }
}
