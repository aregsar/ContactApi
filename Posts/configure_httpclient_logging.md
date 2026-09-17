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

// Register the required redaction services
builder.Services.AddRedaction();
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

// builder.Services.AddHttpClient<MyApiClient>();
// builder.Services.AddHttpClient("MyNamedApiClient")
//     .RedactLoggedHeaders(new[] { "Authorization", "X-Api-Key" });

// builder.Services.AddHttpClient<ITodoClient, TodoClient>(client =>
// {
//     client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
// });


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/client", () => {
    //TODO: Create a HttpClient and make a request to root URL
    //MyApiClient client = new();
    //client.Get("/");

});

app.Run();
```

### Configure Http Client logging using appsettings.json

Add the HttpClientLogging settings to appsettings.json.

Load the settings for AddExtendedHttpClientLogging.

```json
{
"Logging": {
    "LogLevel": {
        "Default": "Information",
        "System.Net.Http.HttpClient": "Information"
    }
},
"HttpClientLogging": {
    "LogRequestStart": true,
    "LogBody": true,
    "BodySizeLimit": 32768,
    "BodyReadTimeout": "00:00:01",
    "RequestPathLoggingMode": "Formatted",
    "RequestPathParameterRedactionMode": "Strict",
    "RequestHeadersDataClasses": {
      "User-Agent": "None",
      "Content-Type": "None",
      "Authorization": "Private"
    },
    "ResponseHeadersDataClasses": {
      "Content-Type": "None",
      "Server": "None"
    },
    "RequestBodyContentTypes": [
      "application/json",
      "text/plain"
    ],
    "ResponseBodyContentTypes": [
      "application/json",
      "text/plain"
    ]
  },
  "HttpClientLoggingNamedPaymentGateway": {
    "LogBody": true,
    "BodySizeLimit": 2048,
    "RequestHeadersDataClasses": {
      "Authorization": "Private",
      "X-Api-Key": "Private"
    }
  },
  "HttpClientLoggingNamedTypedPaymentGateway": {
    "LogBody": true,
    "BodySizeLimit": 65536,
    "RequestHeadersDataClasses": {
      "User-Agent": "None",
      "Accept": "None"
    }
  }
}
```

Add a BuilderExtensions.cs extension file:

```bash
touch ContactHttpLogging/BuilderExtensions.cs
```

TODO: add Builder extension to load httpclient settings and bind to a settings option class
Add the following code to the BuilderExtensions.cs file:

BuilderExtensions.cs

```cs
//dotnet package add Microsoft.Extensions.Http.Diagnostics
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.DependencyInjection;
public static class BuilderExtensions
{
    private static TBuilder AddHttpClientLogging<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Register the required redaction services
        builder.Services.AddRedaction();

        //Strongly typed binding of LoggingOptions to the custom configuration section HttpClientLogging in appsettings.json.
        builder.Services.AddOptions<LoggingOptions>().BindConfiguration("HttpClientLogging");

        //configure all Http Clients
        //The AddExtendedHttpClientLogging implicitly picks up the bound LoggingOptions under the hood.
        builder.Services.AddExtendedHttpClientLogging(options => {});

        // //configure Named Http Client
        // builder.Services.AddOptions<LoggingOptions>("PaymentGateway")
        //                 .BindConfiguration("HttpClientLoggingNamedPaymentGateway");
        // builder.Services.AddHttpClient("PaymentGateway").AddExtendedHttpClientLogging(options => {
        // });

        // //configure Typed Http Client
        // builder.Services.AddOptions<LoggingOptions>(typeof(PaymentGatewayClient).FullName!)
        //                 .BindConfiguration("HttpClientLoggingNamedTypedPaymentGateway");
        // builder.Services.AddHttpClient<PaymentGatewayClient>().AddExtendedHttpClientLogging(options => {
        // });

        return builder;
    }
}
```

> builder.Services.AddOptions<LoggingOptions>().BindConfiguration() loads the settings in the appsettings.json into a LoggingOptions object.
Then under the hood the builder.Services.AddExtendedHttpClientLogging call uses the bound LoggingOptions object.
The AddHttpClientLogging<TBuilder> extension method of the builder wraps these two method calls to make their relationship explicit and encapsulated.
Otherwise the asp.net framework does not provide any indication that the builder.Services.AddOptions<LoggingOptions>().BindConfiguration() call and builder.Services.AddExtendedHttpClientLogging call are related.

Update Program.cs to use the builder.AddHttpClientLogging() extension method instead of builder.Services.AddExtendedHttpClientLogging().

Also moved the builder.Services.AddRedaction() call into builder.AddHttpClientLogging().

Program.cs

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

builder.AddHttpClientLogging();


// builder.Services.AddHttpClient<MyApiClient>();
// builder.Services.AddHttpClient("MyNamedApiClient")
//     .RedactLoggedHeaders(new[] { "Authorization", "X-Api-Key" });

// builder.Services.AddHttpClient<ITodoClient, TodoClient>(client =>
// {
//     client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
// });


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/client", () => {
    //TODO: Create a HttpClient and make a request to root URL
    //MyApiClient client = new();
    //client.Get("/");

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
