## Configure OpenTelemetry

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactOpenTelemetry
dotnet sln ContactApi.slnx add ContactOpenTelemetry/ContactOpenTelemetry.csproj
```

> All following commands will be run from the solution root directory

### Add builder Extension File

Add builder Extension File

```bash
touch ContactOpenTelemetry/BuilderExtensions.cs
```

BuilderExtensions.cs

```cs
namespace Microsoft.Extensions.Hosting;

public static class BuilderExtensions
{
    public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry();

        builder.Services.AddOpenTelemetry()
                        .WithTracing(tracing =>
                        {
                            tracing.AddSource(builder.Environment.ApplicationName)
                                .AddAspNetCoreInstrumentation()
                                // dotnet package add OpenTelemetry.Instrumentation.GrpcNetClient --project ContactOpenTelemetry/ContactOpenTelemetry.csproj)
                                //.AddGrpcClientInstrumentation()
                                .AddHttpClientInstrumentation();
                        });


        builder.Services.AddOpenTelemetry()
                        .WithMetrics(metrics =>
                        {
                            metrics.AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddRuntimeInstrumentation();
                        });


        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;

    }
}
```

Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);
builder.AddOpenTelemetry();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
       "Microsoft.AspNetCore": "Warning"
    },
    "OpenTelemetry": {
      "IncludeScopes": true,
      "IncludeFormattedMessage": false,
      "ParseStateValues": true
    }
  },
  "AllowedHosts": "*",
  "OTEL_SERVICE_NAME": "ContactOpenTelemetry",
  "OTEL_EXPORTER_OTLP_PROTOCOL": "grpc",
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317"
}
```

```bash
touch ContactLoggingProviders/ContactApi.http
```

Add the following content to the .http file:

```http
@baseUrl = http://localhost:5094

### Get Root URL
GET {{baseUrl}}/
Accept: application/json
```

```bash
dotnet run --project ContactOpenTelemetry/ContactOpenTelemetry.csproj --launch-profile http
```
