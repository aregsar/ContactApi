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
dotnet package add Microsoft.EntityFrameworkCore.InMemory --project ContactEfCoreLogging/ContactEfCoreLogging.csproj
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactEfCoreLogging/ContactEfCoreLogging.csproj
dotnet package add Scalar.AspNetCore --project ContactEfCoreLogging/ContactEfCoreLogging.csproj

dotnet package add Microsoft.EntityFrameworkCore.Sqlite --project ContactEfCoreLogging/ContactEfCoreLogging.csproj
dotnet package add Microsoft.EntityFrameworkCore.SqlServer --project ContactEfCoreLogging/ContactEfCoreLogging.csproj
dotnet package add Npgsql.EntityFrameworkCore.PostgreSQL --project ContactEfCoreLogging/ContactEfCoreLogging.csproj
dotnet package add Pomelo.EntityFrameworkCore.MySql --project ContactEfCoreLogging/ContactEfCoreLogging.csproj



```

```cs

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    // options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});


```

## Add logging levels in settings

```json

{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.EntityFrameworkCore.Database.Connection": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Transaction": "Warning",
      "Microsoft.EntityFrameworkCore.ChangeTracking": "Information",
      "Microsoft.EntityFrameworkCore.Infrastructure": "Warning",
      "Microsoft.EntityFrameworkCore.Query": "None",
      "Microsoft.EntityFrameworkCore.Model": "None",
      "Microsoft.EntityFrameworkCore.Migrations": "Warning"
    }
  }
}

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
