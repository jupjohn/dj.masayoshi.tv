using System.Data;
using System.Reflection;
using MasayoshiDj.Generic;
using MasayoshiDj.Observability.Processors;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Proto.OpenTelemetry;

namespace MasayoshiDj.Observability;

public static class OpenTelemetryBuilderExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var serviceName = Environment
            .GetEnvironmentVariable("OTEL_SERVICE_NAME")
            .OrDefaultTo(builder.Environment.ApplicationName);
        var assembly = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

        builder.Services.Configure<StaticFileOptions>(options => {
            options.OnPrepareResponse = _ =>
            {
                // prevents static assets from generating traces. we don't care
                Activity.Current?.IsAllDataRequested = false;
            };
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName,
                serviceInstanceId: Environment.MachineName,
                // FIXME(jupjohn): git hash isn't being pulled through because it's not in the container's build context
                serviceVersion: assembly.ProductVersion
            ))
            .WithLogging(logging => logging.AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddOtlpExporter()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddProtoActorInstrumentation())
            .WithTracing(metrics => metrics
                .AddSource(AppDiagnostics.SourceName)
                .AddProcessor<RemoveProtoActorInternalsProcessor>()
                .AddProcessor<AppendHttpClientPathProcessor>()
                .AddOtlpExporter()
                .AddAspNetCoreInstrumentation(b => b.Filter = c => c.Request.Path != "/healthz")
                .AddEntityFrameworkCoreInstrumentation(b => b.EnrichWithIDbCommand = EnrichEntityFrameworkActivity)
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddProtoActorInstrumentation());

        return builder;
    }

    // TODO(jupjohn): I prefer the format "SQL: <command> <table>"
    private static readonly string[] Commands = ["SELECT", "INSERT", "UPDATE", "DELETE"];
    private static void EnrichEntityFrameworkActivity(Activity activity, IDbCommand command)
    {
        var commandCommandText = command.CommandText;
        var firstCommand = Commands
            .Select(cmd => (
                Command: cmd,
                Index: commandCommandText.IndexOf(cmd, StringComparison.InvariantCultureIgnoreCase)
            ))
            .Where(pair => pair.Index != -1)
            .OrderBy(pair => pair.Index)
            .Select(pair => pair.Command)
            .FirstOrDefault("Query");

        var dbName = activity.GetTagItem("db.name") as string ?? activity.DisplayName;
        activity.DisplayName = $"Database {firstCommand}: {dbName}";
    }
}
