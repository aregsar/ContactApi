# ContactOpenApi

Packages installed:

```bash
dotnet package add Microsoft.EntityFrameworkCore.InMemory --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Scalar.AspNetCore --project ContactOpenApi/ContactOpenApi.csproj
```

```bash
touch ContactOpenApi/Contact.cs
touch ContactOpenApi/ContactsEndpointMapper.cs
touch ContactOpenApi/ContactDbContext.cs
```

```bash
dotnet run --project ContactOpenApi/ContactOpenApi.csproj --launch-profile http
# http://localhost:5037/openapi/v1.json
# http://localhost:5037/scalar/
```
