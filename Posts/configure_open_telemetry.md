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

### Setting up the aspire dashboard standalone

Run the aspire dashboard via docker run:

```bash
# -p 18888:18888: Maps the aspire dashboard http://localhost:18888
# -p 4317:4317: Opens the native gRPC OTLP receiver port.
# -p 4318:4318: Opens the fallback HTTP OTLP receiver port.
# -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true: Skips browser token authentication flags so you do not have to copy-paste secure keys out of docker console strings during local testing


docker run --rm -it -d \
  --name aspire-dashboard \
  -p 18888:18888 \
  -p 4317:4317 \
  -p 4318:4318 \
  -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true \
  ://microsoft.com
```

Or run the aspire dashboard via docker compose:

```yaml
Docker compose:
version: '3.8'
services:
  aspire-dashboard:
    container_name: aspire-dashboard
    image: ://microsoft.com
    ports:
      - "18888:18888" # Dashboard Web UI
      - "4317:4317"   # OTLP gRPC endpoint
      - "4318:4318"   # OTLP HTTP endpoint
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true

```

The dashboard will directly recieve the OLTP exported data on port 4317.
There is no OLTP collector that receives the exported data on port 4317 and forwards it to the dashboard on a different port. So we dont need to run a OpenTelemtry collector.

Add env vars to properties/launchsettings.json http profile instead of appsettings.json:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "OTEL_SERVICE_NAME": "OpenTelemetryDemo",
        "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
        "OTEL_EXPORTER_OTLP_PROTOCOL": "grpc"
      }
    }
  }
}
```

### Using Auth with the dashboard

docker run --rm -it -d \
  --name aspire-dashboard \
  -p 18888:18888 \
  -p 4317:4317 \
  -p 4318:4318 \
  -e DASHBOARD__FRONTEND__AUTHMODE=BrowserToken \
  -e DASHBOARD__FRONTEND__BROWSERTOKEN=MyPassword123 \
  -e DASHBOARD__OTLP__AUTHMODE=ApiKey \
  -e DASHBOARD__OTLP__PRIMARYAPIKEY=MySecretIngestionKey1234 \
  ://microsoft.com

```

Or run the aspire dashboard via docker compose:

```yaml
Docker compose:
version: '3.8'
services:
  aspire-dashboard:
    container_name: aspire-dashboard
    image: ://microsoft.com
    ports:
      - "18888:18888" # Dashboard Web UI
      - "4317:4317"   # OTLP gRPC endpoint
      - "4318:4318"   # OTLP HTTP endpoint
    environment:
      - DASHBOARD__FRONTEND__AUTHMODE=BrowserToken
      - DASHBOARD__FRONTEND__BROWSERTOKEN=MyPassword123
      - DASHBOARD__OTLP__AUTHMODE=ApiKey
      - DASHBOARD__OTLP__PRIMARYAPIKEY=MySecretIngestionKey1234

```

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "OTEL_SERVICE_NAME": "OpenTelemetryDemo",
        "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
        "OTEL_EXPORTER_OTLP_PROTOCOL": "grpc",
        "OTEL_EXPORTER_OTLP_HEADERS":"x-otlp-api-key=ProdSecretIngestionKeyABCDEFG98765"
      }
    }
  }
}
```

Note both the DASHBOARD__OTLP__PRIMARYAPIKEY api key and the x-otlp-api-key header value from OTEL_EXPORTER_OTLP_HEADERS is the same.

x-otlp-api-key header is the header that the OpenTelemetry exporter from out project sends to the <http://localhost:4317> and the dashboard running at that endpoint expects the header to match the DASHBOARD__OTLP__PRIMARYAPIKEY env var that the dashboard service was launched with.

The DASHBOARD__FRONTEND__BROWSERTOKEN is for securing the dashboard UI.
If that env var is set when launching the dashboard service, the dashboard UI will have a form to enter that password to be allowed to view the dashbaord.
