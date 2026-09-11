## Configure Logging Providers

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactLoggingProviders
dotnet sln ContactApi.slnx add ContactLoggingProviders/ContactLoggingProviders.csproj
```

> All following commands will be run from the solution root directory

### Add builder Extension File

```bash
touch ContactLoggingProviders/BuilderExtensions.cs
```

The framework default configuration when WebApplication.CreateBuilder() is called.

```cs
namespace Microsoft.Extensions.Hosting
{
    public static class HostingHostBuilderExtensions
    {
        // This is the underlying internal method that registers the defaults
        internal static ILoggingBuilder AddDefaultLoggingProviders(this ILoggingBuilder builder, HostBuilderContext context)
        {
            // 1. Adds the configuration section mapping (binds Logging configuration)
            builder.AddConfiguration(context.Configuration.GetSection("Logging"));

            // 2. Registers the built-in Console logger
            builder.AddConsole();

            // 3. Registers the cross-platform Debug logger
            builder.AddDebug();

            // 4. Registers cross-platform Event Source logging
            builder.AddEventSourceLogger();

            // 5. Conditionally registers Windows EventLog if running on a Windows OS
            if (OperatingSystem.IsWindows())
            {
                // Binds safely to the Windows Event Log
                builder.AddEventLog();
            }

            return builder;
        }
    }
}
```

Override the default configuration

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
@baseUrl = http://localhost:5014

### Get Root URL
GET {{baseUrl}}/
Accept: application/json
```

```bash
dotnet run --project ContactLoggingProviders.csproj --launch-profile http.prod
```

Send Request

```bash
dotnet run --project ContactLoggingProviders.csproj --launch-profile http
```

Send Request
