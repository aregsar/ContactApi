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

builder.Services.AddRedaction();
// Connect compliance rules specifically to the HTTP Logging Middleware
builder.Services.AddHttpLoggingRedaction(options =>
{
    // Target specific headers, parameters, or pathways to scrub
});

builder.Services.AddHttpLogging();
builder.Services.AddExtendedHttpClientLogging();

builder.Services.AddApplicationLogEnricher(options =>
{
    // Adds Host, Environment, etc.like Environment.CurrentManagedThreadId and Environment.MachineName
    options.InitializeServiceLogState = true;
});

builder.Services.AddLogEnricher<CustomLogEnricher>();
builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();
builder.Services.AddHttpClientLogEnricher<CustomHttpClientLogEnricher>();

builder.Services.AddHttpLoggingInterceptor<CustomHttpLoggingInterceptor>();



var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```
