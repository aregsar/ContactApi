## Configure OpenTelemetry

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactApi
dotnet new web -o ContactLoggingProviders
dotnet sln ContactApi.slnx add ContactLoggingProviders/ContactLoggingProviders.csproj
```

> All following commands will be run from the solution root directory

### Add builder Extension File
