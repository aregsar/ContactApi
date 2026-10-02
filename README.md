# ContactApi Projects

Creating a new project for a blog post

```bash
dotnet new web -o ContactApi
dotnet sln ContactApi.slnx add ContactApi/ContactApi.csproj
echo "# ContactApi" >> ContactApi/ContactApi.md


dotnet new web -o ContactStarter
dotnet sln ContactApi.slnx add ContactStarter/ContactStarter.csproj
echo "# ContactStarter" >> ContactStarter/ContactStarter.md

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


dotnet new web -o ContactEfCoreLogging
dotnet sln ContactApi.slnx add ContactEfCoreLogging/ContactEfCoreLogging.csproj
echo "# ContactEfCoreLogging" >> ContactEfCoreLogging/ContactEfCoreLogging.md

# https://app.pluralsight.com/ilx/video-courses/building-data-driven-asp-dot-net-core-10-application-ef-core/course-overview

# https://app.pluralsight.com/ilx/video-courses/getting-started-ef-core-10/course-overview


dotnet new web -o ContactUseStatusCodes
dotnet sln ContactApi.slnx add ContactUseStatusCodes/ContactUseStatusCodes.csproj
echo "# ContactUseStatusCodes" >> ContactUseStatusCodes/ContactUseStatusCodes.md

dotnet new web -o ContactUseExceptions
dotnet sln ContactApi.slnx add ContactUseExceptions/ContactUseExceptions.csproj
echo "# ContactUseExceptions" >> ContactUseExceptions/ContactUseExceptions.md

dotnet new web -o ContactGlobalExceptions
dotnet sln ContactApi.slnx add ContactGlobalExceptions/ContactGlobalExceptions.csproj
echo "# ContactGlobalExceptions" >> ContactGlobalExceptions/ContactGlobalExceptions.md

dotnet new web -o ContactValidationExceptions
dotnet sln ContactApi.slnx add ContactValidationExceptions/ContactValidationExceptions.csproj
echo "# ContactValidationExceptions" >> ContactValidationExceptions/ContactValidationExceptions.md

dotnet new web -o ContactProblemDetails
dotnet sln ContactApi.slnx add ContactProblemDetails/ContactProblemDetails.csproj
echo "# ContactProblemDetails" >> ContactProblemDetails/ContactProblemDetails.md

dotnet new web -o ContactHealthChecks
dotnet sln ContactApi.slnx add ContactHealthChecks/ContactHealthChecks.csproj
echo "# ContactHealthChecks" >> ContactHealthChecks/ContactHealthChecks.md

dotnet new web -o ContactAspire
dotnet sln ContactApi.slnx add ContactAspire/ContactAspire.csproj
echo "# ContactAspire" >> ContactAspire/ContactAspire.md
```
