## Configure Logging Providers

In this post we will override the default logging providers to create our own efficient logging configuration.

We will be removing all providers for production since we will be using OpenTelemetry logging described in a followup post.

We only add the console and debug loggers when in development.

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactLoggingProviders
dotnet sln ContactApi.slnx add ContactLoggingProviders/ContactLoggingProviders.csproj
```

> All following commands will be run from the solution root directory

### The default logging provider configuration

The framework default configuration when WebApplication.CreateBuilder() is called calls AddDefaultLoggingProviders() that adds default logging providers.

```cs
namespace Microsoft.Extensions.Hosting
{
    public static class HostingHostBuilderExtensions
    {
        internal static ILoggingBuilder AddDefaultLoggingProviders(this ILoggingBuilder builder, HostBuilderContext context)
        {
            builder.AddConfiguration(context.Configuration.GetSection("Logging"));

            builder.AddConsole();

            builder.AddDebug();

            builder.AddEventSourceLogger();

            if (OperatingSystem.IsWindows())
            {
                builder.AddEventLog();
            }

            return builder;
        }
    }
}
```

### Override the default configuration

Add builder Extension File

```bash
touch ContactLoggingProviders/BuilderExtensions.cs
```

BuilderExtensions.cs

```cs
namespace Microsoft.Extensions.Hosting;

public static class BuilderExtensions
{
    public static TBuilder AddLoggingProviders<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.ClearProviders();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        return builder;
    }
}
```

Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);
builder.AddLoggingProviders();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

ContactLoggingProviders/Properties/launchSettings.json:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5094",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "http.prod": {
        "commandName": "Project",
        "dotnetRunMessages": true,
        "launchBrowser": true,
        "applicationUrl": "http://localhost:5094",
        "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Production"
        }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7223;http://localhost:5094",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

```bash
touch ContactLoggingProviders/ContactApi.http
```

Add the following content to the .http file:

```http
@baseUrl = http://localhost:5094

### Get Root URL
GET {{baseUrl}}/
Accept: application/json
```

```bash
dotnet run --project ContactLoggingProviders/ContactLoggingProviders.csproj --launch-profile http
```

You will see log output in console

Send Request to root URL to verify its running

```bash
dotnet run --project ContactLoggingProviders/ContactLoggingProviders.csproj --launch-profile http.prod
```

You will not see log output in console since all the logging providers were cleared and the console logger was not added for Production environment.

Send Request to root URL to verify its running
