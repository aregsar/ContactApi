# ContactApi Projects

Creating a new project for a blog post

```bash
dotnet new web -o ContactApi
echo "# ContactApi" >> ContactApi/ContactApi.md

dotnet new web -o ContactLoggingProviders
echo "# ContactLoggingProviders" >> ContactLoggingProviders/ContactLoggingProviders.md

dotnet new web -o ContactOpenTelemetry
echo "# ContactOpenTelemetry" >> ContactOpenTelemetry/ContactOpenTelemetry.md

dotnet new web -o ContactHttpLogging
echo "# ContactHttpLogging" >> ContactHttpLogging/ContactHttpLogging.md

///////////////

dotnet new web -o ContactHttpClientLogging
echo "# ContactHttpClientLogging" >> ContactHttpClientLogging/ContactHttpClientLogging.md

dotnet new web -o ContactApplicationLogging
echo "# ContactApplicationLogging" >> ContactApplicationLogging/ContactApplicationLogging.md

dotnet new web -o ContactOpenApi
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

///////////////
dotnet new web -o ContactEfCoreLogging
echo "# ContactEfCoreLogging" >> ContactEfCoreLogging/ContactEfCoreLogging.md

dotnet new web -o ContactUseStatusCodes
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactUseExceptions
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactGlobalExceptions
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactValidationExceptions
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactProblemDetails
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactConfigurationSources
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactOpenTelemetryCollector
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactHealthChecks
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

dotnet new web -o ContactHttpClientConfig
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md
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
