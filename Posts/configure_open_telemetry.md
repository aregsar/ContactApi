## Configure OpenTelemetry

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactOpenTelemetry
dotnet sln ContactApi.slnx add ContactOpenTelemetry/ContactOpenTelemetry.csproj
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash
dotnet package add OpenTelemetry.Extensions.Hosting --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Instrumentation.AspNetCore --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Instrumentation.Http --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Instrumentation.Runtime --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Exporter.OpenTelemetryProtocol --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Instrumentation.EntityFrameworkCore --prerelease --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
dotnet package add OpenTelemetry.Instrumentation.GrpcNetClient --prerelease --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
```

### Add a Trace enricher File

Add GlobalTraceEnricher File

```bash
touch ContactOpenTelemetry/GlobalTraceEnricher.cs
```

```cs
using OpenTelemetry.Trace;
using System.Diagnostics;

public class GlobalTraceEnricher : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        //Enrich activity with custom data
        //Example
        //activity.SetTag("machine.architecture", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
    }
}
```

### Add builder Extension File

Add builder Extension File

```bash
touch ContactOpenTelemetry/BuilderExtensions.cs
```

BuilderExtensions.cs

```cs
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;


public static class BuilderExtensions
{
    public const string CustomSourceName = "MyCustomActivitySource";
    public static readonly ActivitySource MyActivitySource = new(CustomSourceName);

    public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry();

        builder.Services.AddOpenTelemetry()
                        .WithTracing(tracing =>
                        {
                            tracing.AddSource(CustomSourceName)
                                    .AddAspNetCoreInstrumentation(options =>
                                    {
                                        options.EnrichWithHttpRequest = (activity, request) =>
                                        {
                                            //Enrich the activity with request data
                                            //Example:
                                            //activity.SetTag("request.accept_header", request.HttpContext.Request.Headers["accept"]);

                                        };

                                        // 2. Enrich using the outgoing HTTP Response
                                        options.EnrichWithHttpResponse = (activity, response) =>
                                        {
                                            //Enrich the activity with response data
                                            //Example:
                                            //activity.SetTag("response.status", response.Status);

                                        };
                                    })
                                    .AddProcessor<GlobalTraceEnricher>()
                                    .AddEntityFrameworkCoreInstrumentation(options =>
                                    {
                                        // .NET 10 Native tracing configurations
                                        options.SetDbStatementForText = true;
                                    })
                                    .AddGrpcClientInstrumentation()
                                    .AddHttpClientInstrumentation();
                        });


        builder.Services.AddOpenTelemetry()
                        .WithMetrics(metrics =>
                        {
                            metrics.AddMeter("Microsoft.EntityFrameworkCore")
                                    .AddAspNetCoreInstrumentation()
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
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.AddOpenTelemetry();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/trace", () =>
{
    using Activity? activity = BuilderExtensions.MyActivitySource.StartActivity("ProcessOrderEndpoint");
    activity?.SetTag("id", 1);
    activity?.SetStatus(ActivityStatusCode.Error, "User not found");
});

app.Run();
```

appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
       "Microsoft.AspNetCore": "Warning",
       "Microsoft.EntityFrameworkCore": "Warning",
       "Microsoft.EntityFrameworkCore.Database.Command": "Information",
       "Microsoft.EntityFrameworkCore.Database.Connection": "Information",
       "Microsoft.EntityFrameworkCore.Database.Transaction": "Information"
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
touch ContactOpenTelemetry/ContactApi.http
```

Add the following content to the .http file:

```http
@baseUrl = http://localhost:5042

### Get Root URL
GET {{baseUrl}}/
Accept: application/json

### Get Trace URL
GET {{baseUrl}}/trace
Accept: application/json
```

Run the project:

```bash
dotnet run --project ContactOpenTelemetry/ContactOpenTelemetry.csproj --launch-profile http
```
