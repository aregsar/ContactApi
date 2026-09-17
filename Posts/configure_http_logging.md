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

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestQuery |
        HttpLoggingFields.RequestHeaders |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Configuring Http Logging using appsettings.json settings

In order to be able to change the properties that we can log dynamically at runtime instead requiring a application rebuild, we will use appsettings.json to configure our http logging with a few overrides in code for development environment.

Add the  HttpLogging section to appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
       "Microsoft.AspNetCore": "Warning"
    }
  },
  "HttpLogging": {
    "LoggingFields": "Duration,
        RequestProperties,
        ResponseProperties,
        RequestHeaders,
        ResponseHeaders,
        RequestBody,
        ResponseBody,
        ResponseStatusCode,
        RequestQuery,
        RequestPath,
        RequestMethod",
    "RequestBodyLogLimit": 32768,
    "ResponseBodyLogLimit": 32768,
    "CombineLogs": true,
    "RequestHeaders": [
      "Accept",
      "Content-Type",
      "User-Agent",
      "X-API-Version",
    ],
    "ResponseHeaders": [
      "Content-Type",
      "Server"
    ],
    "MediaTypeOptions": {
      "Clear": false,
      "SupportedMediaTypes": [
        "application/json",
        "text/plain",
        "application/xml",
        "application/problem+json"
      ]
    }
  },
  "AllowedHosts": "*"
}
```

Add a BuilderExtensions.cs extension file:

```bash
touch ContactHttpLogging/BuilderExtensions.cs
```

Add the following code to the BuilderExtensions.cs file:

BuilderExtensions.cs

```cs
using Microsoft.AspNetCore.HttpLogging;//package built into sdk
using Microsoft.Extensions.DependencyInjection;

public static class BuilderExtensions
{
    private static TBuilder AddHttpLogging<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        //Strongly typed binding of HttpLoggingOptions to the custom configuration section HttpLogging in appsettings.json.
        builder.Services.AddOptions<HttpLoggingOptions>().BindConfiguration("HttpLogging");

        //The AddHttpLogging implicitly picks up the bound HttpLoggingOptions under the hood and
        //in development mode overrides the some of the settings in code.
        builder.Services.AddHttpLogging(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                //overriding the HttpLoggingOptions in development

                options.RequestBodyLogLimit = 1024 * 32;
                options.ResponseBodyLogLimit = 1024 * 32;

                //example of how to override RequestHeaders and ResponseHeaders
                // options.RequestHeaders ??= new HashSet<string>();
                // options.RequestHeaders.Clear();
                // options.RequestHeaders.Add("User-Agent");
                //
                // options.ResponseHeaders ??= new HashSet<string>();
                // options.ResponseHeaders.Clear();
                // options.ResponseHeaders.Add("Content-Type");
            }
        });

        return builder;
    }
}
```

> builder.Services.AddOptions<HttpLoggingOptions>().BindConfiguration() loads the settings in the appsettings.json into a HttpLoggingOptions object.
Then under the hood the builder.Services.AddHttpLogging call uses the HttpLoggingOptions object.
The AddHttpLogging<TBuilder> extension method of the builder wraps these two method calls to make their relationship explicit and encapsulated.
Otherwise the asp.net framework does not provide any indication that the two calls are related.

Update Program.cs to use the builder.AddHttpLogging() extension method instead of builder.Services.AddHttpLogging().

Program.cs:

```cs
// using Microsoft.AspNetCore.Diagnostics.Logging;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Http.Diagnostics;
// using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.AddHttpLogging();

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

builder.AddHttpLogging();

builder.Services.AddHttpLoggingRedaction(options =>
{
    // redact specific headers, parameters and paths
});

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
        //Capture basic path or routing context
        collector.Add("http.request.path", httpContext.Request.Path.Value ?? "/");
        collector.Add("http.request.method", httpContext.Request.Method);

        //enrich using the httpContext.Request
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            collector.Add("http.user_agent", userAgent);
        }
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            collector.Add("http.custom.correlation_id", correlationId.ToString());
        }

        //Enrich with authentication info
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                collector.Add("user.id", userId);
            }
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

builder.AddHttpLogging();

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

builder.AddHttpLogging();

builder.Services.AddHttpLoggingRedaction(options => { });

builder.Services.AddHttpLogEnricher<CustomHttpLogEnricher>();

builder.Services.AddHttpLoggingInterceptor<CustomHttpLoggingInterceptor>();

var app = builder.Build();

app.UseHttpLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Disabling Http Logging middleware

appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "None"
    }
  }
}
```

### Controlling the log level for http and Host logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
       "Microsoft.AspNetCore.Hosting.Diagnostics": "Warning",
       "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Warning"
    }
  }
}
```
