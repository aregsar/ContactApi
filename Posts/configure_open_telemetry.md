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

## Setting up the aspire dashboard standalone

There are multiple ways to setup the aspire dashboard to run in standalone mode.

We will cover three ways below:

### Running the dashboard natively (Mac and Windows only)

Using the aspire cli

Downnload the aspire cli

```bash
####

```

Run the dashboard command to launch the aspire dashboard:

```bash
aspire dashboard run --allow-anonymous
```

When we run the dashboard on our host it ingests exported data on localhost:4317.

Our application OpenTelemetry exports to localhost:4317 by default.

The dashboard should be available at <http://localhost:18888>

### Running the dashboard as a docker container

We can also run the official aspire dashboard docker container using docker compose

When we run the dashboard in a docker container it ingest the exported data on port 18889 by default

OpenTelemetry exports data to port 4317 by default so we have to map it to the internal port 18889 for the dashboard.

```yml
services:
  aspire-dashboard:
    image: ://microsoft.com
    container_name: aspire-dashboard
    ports:
      - "18888:18888" # Dashboard Web UI
      - "4317:18889"  # Maps standard GRPC OTLP host port 4317 to internal dashboard port 18889
      - "4318:18890" # Maps standard HTTP OTLP host port 4317 to internal dashboard port 18889
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true
```

```bash
docker run --rm -it \
  -p 18888:18888 \
  -p 4317:18889 \
  -p 4318:18890 \
  --name aspire-dashboard \
  mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

The dashboard should be available at <http://localhost:18888>

### Running the OpenTelemetry Collector and aspire dashboard together

In production setups the OT is generally exported to a central collector service.

The collector service then distributes the exported data to multiple other specialized dashboard/analytics systems.

We can simulate this locally by using Docker compose to run the collector and dashboard a two separate containers.

The aspire dashboard in this case acts one of the dashboard/analytics systems that the collector distributes it data to.

A otel-collector-config.yaml file mounted to the collector container volume configures the collector to receive the OTexported data on the standard OT port 4317 that our application exports to.

The collector configuration also specified that the collector distribute the data to aspire-dashboard:18889 over the internal docker compose network to the dashboard container.

```bash
touch docker-compose.yaml
```

```yml
services:
  aspire-dashboard:
    image: ://microsoft.com
    container_name: aspire-dashboard
    ports:
      - "18888:18888" # Dashboard Web UI
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    container_name: otel-collector
    volumes:
      - ./otel-collector-config.yaml:/etc/otelcol-contrib/config.yaml # Fixed default path for -contrib image
    ports:
      - "4317:4317" # Exposed to your host machine's ASP.NET app (gRPC)
      - "4318:4318" # Exposed to your host machine's ASP.NET app (HTTP)
    depends_on:
      - aspire-dashboard
```

The collector configuration yaml file

```bash
touch otel-collector-config.yaml
```

otel-collector-config.yaml

```yml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:

exporters:
  otlp/aspire:
    endpoint: "aspire-dashboard:18889" # Internal Docker routing
    tls:
      insecure: true

service:
  processors: [batch]
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
    logs:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/aspire]
```

The dashboard should be available at <http://localhost:18888>

### Using Auth with the dashboard (REMOVE)

```yaml
Docker compose:
version: '3.8'
services:
  aspire-dashboard:
    image: ://microsoft.com
    container_name: aspire-dashboard
    ports:
      - "18888:18888" # Dashboard Web UI
      - "4317:18889"  # Maps standard GRPC OTLP host port 4317 to internal dashboard port 18889
      - "4318:18890" # Maps standard HTTP OTLP host port 4317 to internal dashboard port 18889
    environment:
      - DASHBOARD__FRONTEND__AUTHMODE=BrowserToken
      - DASHBOARD__FRONTEND__BROWSERTOKEN=MyPassword123
      - DASHBOARD__OTLP__AUTHMODE=ApiKey
      - DASHBOARD__OTLP__PRIMARYAPIKEY=MySecretIngestionKey1234

```

Add the OTEL_EXPORTER_OTLP_HEADERS to the appsettings.json file:

Here are the env vars at the root of the appsettings.json file:

```json
{
"OTEL_SERVICE_NAME": "OpenTelemetryDemo",
"OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
"OTEL_EXPORTER_OTLP_PROTOCOL": "grpc",
"OTEL_EXPORTER_OTLP_HEADERS":"x-otlp-api-key=ProdSecretIngestionKeyABCDEFG98765"
}
```

Note both the DASHBOARD__OTLP__PRIMARYAPIKEY api key and the x-otlp-api-key header value from OTEL_EXPORTER_OTLP_HEADERS is the same.

x-otlp-api-key header is the header that the OpenTelemetry exporter from out project sends to the <http://localhost:4317> and the dashboard running at that endpoint expects the header to match the DASHBOARD__OTLP__PRIMARYAPIKEY env var that the dashboard service was launched with.

The DASHBOARD__FRONTEND__BROWSERTOKEN is for securing the dashboard UI.
If that env var is set when launching the dashboard service, the dashboard UI will have a form to enter that password to be allowed to view the dashbaord.
