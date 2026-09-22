# ContactApi Projects

Creating a new project for a blog post

```bash
dotnet new web -o ContactApi
dotnet sln ContactApi.slnx add ContactApi/ContactApi.csproj
echo "# ContactApi" >> ContactApi/ContactApi.md

dotnet new web -o ContactLoggingProviders
dotnet sln ContactApi.slnx add ContactLoggingProviders/ContactLoggingProviders.csproj
echo "# ContactLoggingProviders" >> ContactLoggingProviders/ContactLoggingProviders.md

dotnet new web -o ContactOpenTelemetry
dotnet sln ContactApi.slnx add ContactOpenTelemetry/ContactOpenTelemetry.csproj
echo "# ContactOpenTelemetry" >> ContactOpenTelemetry/ContactOpenTelemetry.md

dotnet new web -o ContactHttpLogging
dotnet sln ContactApi.slnx add ContactHttpLogging/ContactHttpLogging.csproj
echo "# ContactHttpLogging" >> ContactHttpLogging/ContactHttpLogging.md

///////////////

dotnet new web -o ContactHttpClientLogging
dotnet sln ContactApi.slnx add ContactHttpClientLogging/ContactHttpClientLogging.csproj
echo "# ContactHttpClientLogging" >> ContactHttpClientLogging/ContactHttpClientLogging.md

dotnet new web -o ContactApplicationLogging
dotnet sln ContactApi.slnx add ContactApplicationLogging/ContactApplicationLogging.csproj
echo "# ContactApplicationLogging" >> ContactApplicationLogging/ContactApplicationLogging.md

dotnet new web -o ContactOpenApi
dotnet sln ContactApi.slnx add ContactOpenApi/ContactOpenApi.csproj
echo "# ContactOpenApi" >> ContactOpenApi/ContactOpenApi.md

///////////////
dotnet new web -o ContactEfCoreLogging
dotnet sln ContactApi.slnx add ContactEfCoreLogging/ContactEfCoreLogging.csproj
echo "# ContactEfCoreLogging" >> ContactEfCoreLogging/ContactEfCoreLogging.md

# https://app.pluralsight.com/ilx/video-courses/building-data-driven-asp-dot-net-core-10-application-ef-core/course-overview

# https://app.pluralsight.com/ilx/video-courses/getting-started-ef-core-10/course-overview

# https://app.pluralsight.com/ilx/video-courses/ef-core-8-fundamentals/course-overview


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

dotnet new web -o ContactHealthChecks
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
