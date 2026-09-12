## Configure Http Logging

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactHttpLogging
dotnet sln ContactApi.slnx add ContactHttpLogging/ContactHttpLogging.csproj
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash
dotnet package add Microsoft.Extensions.Http.Diagnostics --project ContactOpenTelemetry/ContactOpenTelemetry.csproj




dotnet package add Microsoft.Extensions.Diagnostics.Enrichment
#for builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();
dotnet package add Microsoft.AspNetCore.Diagnostics.Middleware
dotnet package add Microsoft.Extensions.Compliance.Redaction
#for AddExtendedHttpClientLogging
```

```bash
cat ContactApi/Program.cs
```

Program.cs:

```cs
// using Microsoft.AspNetCore.Diagnostics.Logging;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Diagnostics.Enrichment;
// using Microsoft.Extensions.Hosting;
// using System.Security.Claims;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

/////////////////////////////////////////////////////
// Http Logging
// Register core server logging middleware
builder.Services.AddHttpLogging(options => { });

// Register and configure redaction for that middleware
builder.Services.AddHttpLoggingRedaction(options => {
    // Redaction configuration goes here
});
/////////////////////////////////////////////////////


/////////////////////////////////////////////////////
// HttpClient Logging
//TODO: load settings from config
builder.Services.AddExtendedHttpClientLogging();
builder.Services.AddRedaction();
///////////////////////////////////////////////////



///////////////////////////////////////
//ENRICHING
//
// 1. Turn on the enrichment subsystem (required for all enrichers)
//dotnet package add Microsoft.Extensions.Telemetry --project xxxxx
builder.Logging.EnableEnrichment();

// 2. Load from configuration AND apply code overrides
builder.Services.AddApplicationLogEnricher(builder.Configuration.GetSection("ApplicationLogEnricherOptions"))
                .Configure<Microsoft.Extensions.Telemetry.Logging.ApplicationLogEnricherOptions>(options =>
                {
                    // This explicitly overrides whatever was in the appsettings JSON file
                    options.ApplicationName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? builder.Environment.ApplicationName;

                    // Adds Host, Environment, etc.like Environment.CurrentManagedThreadId and Environment.MachineName
                    options.InitializeServiceLogState = true;

                    if (!string.IsNullOrEmpty(otelServiceVersion))
                    {
                        // Tells the enricher to include version metadata
                        options.ServiceVersion = true;

                        // Sets the exact version string to match your OTel environment variable
                        // (Note: depending on the library version, this may map to AmbientMetadata or an environment lookup)
                        Environment.SetEnvironmentVariable("APPLICATION_BUILD_VERSION", Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION"));
                    }
                });
///////////////////////////////////////

//builder.Services.AddLogEnricher<CustomLogEnricher>();
//builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();
//builder.Services.AddHttpClientLogEnricher<CustomHttpClientLogEnricher>();
//builder.Services.AddHttpLoggingInterceptor<CustomHttpLoggingInterceptor>();




var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

```json
{
  "AmbientMetadata": {
    "Application": {
      "ApplicationName": "PaymentService",
      "BuildVersion": "2.4.1",
      "DeploymentRing": "canary"
    }
  },
  "ApplicationLogEnricherOptions": {
    "BuildVersion": true,
    "DeploymentRing": true
  }
}

```
