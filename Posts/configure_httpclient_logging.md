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
builder.Services.AddExtendedHttpClientLogging();
builder.Services.AddRedaction();


var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.MapGet("/client", () => {
    //TODO: Create a HttpClient and make a request
});

app.Run();
```

### Adding HttpClient Enrichment

Create CustomHttpClientLogEnricher file:

```bash

touch ContactHttpClientLogging/CustomHttpClientLogEnricher.cs
```

Add code to CustomHttpClientLogEnricher.cs

```cs
//TODO: Add Enricher class

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
