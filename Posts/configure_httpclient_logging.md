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

### Adding HttpClient logging (with mandatory redaction)

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


//TODO: load settings from config
builder.Services.AddExtendedHttpClientLogging(options =>
{
    // Log request headers with data classification
    options.RequestHeadersDataClasses.Add("User-Agent", DataClassification.None);
    options.RequestHeadersDataClasses.Add("Authorization", DataClassification.Unknown);

    // Log response headers
    options.ResponseHeadersDataClasses.Add("Content-Type", DataClassification.None);

    // Enable request/response body logging (use carefully in production)
    options.LogBody = true;

    // Limit request/response body reading to 4KB
    options.BodySizeLimit = 4096;

    // Ensure that application/json is considered a content type we want to log
    options.RequestBodyContentTypes.Add("application/json");
    options.ResponseBodyContentTypes.Add("application/json");
});




builder.Services.AddRedaction();


var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.MapGet("/client", () => {
    //TODO: Create a HttpClient and make a request
});

app.Run();
```

### Configure Http Client logging using appsettings.json

Use for AddExtendedHttpClientLogging settings from appsettings.json

```json

{
  "HttpClientLogging": {
    "LogRequestStart": false,
    "LogBody": false,
    "BodySizeLimit": 32768,
    "BodyReadTimeout": "00:00:01",
    "RequestHeadersDataClasses": {
      "User-Agent": "None",
      "Content-Type": "None"
    },
    "ResponseHeadersDataClasses": {
      "Content-Type": "None"
    },
    "RequestPathLoggingMode": "Formatted",
    "RequestPathParameterRedactionMode": "Strict"
  }
}
```

TODO: add Builder extension to load httpclient settings and bind to a settings option class

BuilderExtensions.cs

```cs
//TODO: load settings from config

```

Program.cs

```cs
//TODO: switch to using BuilderExtensions AddHttpClientLogging() extension method
```

### Adding HttpClient Enrichment

Create CustomHttpClientLogEnricher file:

```bash

touch ContactHttpClientLogging/CustomHttpClientLogEnricher.cs
```

Add code to CustomHttpClientLogEnricher.cs

```cs
public class CustomHttpClientLogEnricher : IHttpClientLogEnricher
{
    // Evaluates right before the HTTP request is transmitted over the wire
    public void Enrich(IEnrichmentTagCollector collector, HttpRequestMessage request)
    {
        // 1. Capture the destination host and scheme safely
        if (request.RequestUri != null)
        {
            collector.Add("http.client.host", request.RequestUri.Host);
            collector.Add("http.client.path", request.RequestUri.AbsolutePath);
        }

        // 2. Track outgoing request methods
        collector.Add("http.client.method", request.Method.Method);

        // 3. Inject tracing/correlation hints if custom routing is applied
        if (request.Headers.Contains("X-Target-Service"))
        {
            var targetService = request.Headers.GetValues("X-Target-Service");
            collector.Add("http.client.target_service", string.Join(",", targetService));
        }
    }
}
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

//TODO: load settings from config
builder.Services.AddExtendedHttpClientLogging();
builder.Services.AddRedaction();


//enables enrichment subsystme
//Is this required for httpclient enrichment ???
//builder.Logging.EnableEnrichment();

builder.Services.AddHttpClientLogEnricher<CustomHttpClientLogEnricher>();

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```
