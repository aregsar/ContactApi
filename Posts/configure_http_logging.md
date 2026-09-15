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
dotnet package add Microsoft.Extensions.Diagnostics.Enrichment --project ContactOpenTelemetry/ContactOpenTelemetry.csproj
#for builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();

#dotnet package add Microsoft.AspNetCore.Diagnostics.Middleware
#dotnet package add Microsoft.Extensions.Compliance.Redaction
#for AddExtendedHttpClientLogging
```

```bash
cat ContactApi/Program.cs
```

### Adding Http logging

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

///
// Http Logging
// Register core server logging middleware
builder.Services.AddHttpLogging(options => { });

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Adding Http logging redaction

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

///
// Http Logging
// Register core server logging middleware
builder.Services.AddHttpLogging(options => { });

// Register and configure redaction for that middleware
builder.Services.AddHttpLoggingRedaction(options => {
    // Redaction configuration goes here
});


var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Add Http Logging Enrichement

```cs
//Turn on the enrichment logging subsystem
//Is this required for http logging enrichement ???
//builder.Logging.EnableEnrichment();

//builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();
```

### Add Http Logging Interception

```cs
//builder.Services.AddHttpLoggingInterceptor<CustomHttpLoggingInterceptor>();
```
