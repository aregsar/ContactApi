# ContactEfCoreLogging

## Configure Http Logging

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
cd ContactEfCoreLogging
dotnet new web -o ContactEfCoreLogging
dotnet sln ContactApi.slnx add ContactEfCoreLogging/ContactEfCoreLogging.csproj
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash
dotnet package add Microsoft.EntityFrameworkCore.InMemory --project ContactEfCoreLogging/ContactOpenApi.csproj
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactEfCoreLogging/ContactOpenApi.csproj
dotnet package add Scalar.AspNetCore --project ContactEfCoreLogging/ContactOpenApi.csproj

dotnet package add Microsoft.EntityFrameworkCore.Sqlite --project ContactEfCoreLogging/ContactOpenApi.csproj
dotnet package add Npgsql.EntityFrameworkCore.PostgreSQL --project ContactEfCoreLogging/ContactOpenApi.csproj
dotnet package add Pomelo.EntityFrameworkCore.MySql --project ContactEfCoreLogging/ContactOpenApi.csproj
dotnet package add Microsoft.EntityFrameworkCore.SqlServer --project ContactEfCoreLogging/ContactOpenApi.csproj

.UseSqlite()
.UseSqlServer()
.UseNpgsql()
.UseMySql()

```

## Add the Contacts API

```bash
touch ContactEfCoreLogging/Contact.cs
touch ContactEfCoreLogging/ContactsEndpointMapper.cs
touch ContactEfCoreLogging/ContactDbContext.cs
```

## Add the OpenAPI services and middleware

```cs
//Program.cs file
```
