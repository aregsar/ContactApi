## Configure OpenTelemetry

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactOpenTelemetry
dotnet sln ContactApi.slnx add ContactOpenTelemetry/ContactOpenTelemetry.csproj
```

> All following commands will be run from the solution root directory

### Add builder Extension File

Add builder Extension File

```bash
touch ContactOpenTelemetry/BuilderExtensions.cs
```

BuilderExtensions.cs

```cs
namespace Microsoft.Extensions.Hosting;

public static class BuilderExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddLoggingProviders();
        builder.AddOpenTelemetry();
    }

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

    public static TBuilder AddOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {

    }
}
```
