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
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

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
// using Microsoft.Extensions.Hosting;
// using System.Security.Claims;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options => { });

builder.Services.AddHttpLoggingRedaction(options => { });

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Add Http Logging Enrichement

Add a CustomHttpLogEnricher file:

```bash

touch ContactHttpLogging/CustomHttpLogEnricher.cs
```

Add the code to CustomHttpLogEnricher.cs

```cs
using Microsoft.AspNetCore.Diagnostics.Middleware;
using Microsoft.Extensions.Diagnostics.Enrichment;
using Microsoft.AspNetCore.Http;

public class CustomHttpLogEnricher : IHttpLogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector, HttpContext httpContext)
    {
        //enrich using the httpContext.Request
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            collector.Add("http.user_agent", userAgent);
        }

        //enrich using the httpContext.Response
        if (httpContext.Response.Headers.TryGetValue("X-Custom-Header", out var customHeaderValue))
        {
            collector.Add("http.response.custom_header", customHeaderValue.ToString());
        }
    }
}
```

Add the CustomHttpLogEnricher to the application services:

```cs
//Turn on the enrichment logging subsystem
//Is this required for http logging enrichement ???
//builder.Logging.EnableEnrichment();

// using Microsoft.AspNetCore.Diagnostics.Logging;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using System.Security.Claims;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options => { });

builder.Services.AddHttpLoggingRedaction(options => { });

builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Add Http Logging Interception

Add a CustomHttpLoggingInterceptor file:

```bash
touch ContactHttpLogging/CustomHttpLoggingInterceptor.cs
```

Add the code to CustomHttpLoggingInterceptor.cs

```cs
public class CustomHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext context)
    {

        context.HttpContext.Request.Headers.Remove("My-Request-Header");

        context.AddParameter("random-request-param", Guid.NewGuid().ToString());

        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext context)
    {

        logContext.HttpContext.Response.Headers.Remove("My-Response-Header");

        context.AddParameter("random-response-param", Guid.NewGuid().ToString());

        return ValueTask.CompletedTask;
    }
}
```

Add the CustomHttpLoggingInterceptor to the application services:

```cs
//Turn on the enrichment logging subsystem
//Is this required for http logging enrichement ???
//builder.Logging.EnableEnrichment();

// using Microsoft.AspNetCore.Diagnostics.Logging;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using System.Security.Claims;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options => { });

builder.Services.AddHttpLoggingRedaction(options => { });

builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();

builder.Services.AddHttpLoggingInterceptor<CustomHttpLoggingInterceptor>();

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```
