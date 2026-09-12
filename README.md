# ContactApi

Creating a new project for a blog post

```bash
dotnet new web -o ContactApi
dotnet new web -o ContactLoggingProviders
dotnet new web -o ContactOpenTelemetry
dotnet new web -o ContactHttpLogging
dotnet new web -o ContactHttpClientLogging
dotnet new web -o ContactEfCoreLogging
dotnet new web -o ContactOpenApi
dotnet new web -o ContactUseStatusCodes
dotnet new web -o ContactUseExceptions
dotnet new web -o ContactGlobalExceptions
dotnet new web -o ContactValidationExceptions
dotnet new web -o ContactProblemDetails
dotnet new web -o ContactLoggingProviders
dotnet new web -o ContactConfigurationSources
dotnet new web -o ContactOpenTelemetryCollector
dotnet new web -o ContactHealthChecks
dotnet new web -o ContactHttpClientConfig

```

```cs
builder.Services.AddDbContext<MyDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .options.EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
    {
        //Show query parameter values
        options.EnableSensitiveDataLogging();
    }
);
```
