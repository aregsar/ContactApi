# ContactOpenApi

Packages installed:

```bash
dotnet package add Microsoft.EntityFrameworkCore.InMemory --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Scalar.AspNetCore --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Microsoft.AspNetCore.Authentication.JwtBearer --project ContactOpenApi/ContactOpenApi.csproj
dotnet package add Microsoft.OpenApi --project ContactOpenApi/ContactOpenApi.csproj

```

```bash
touch ContactOpenApi/Contact.cs
touch ContactOpenApi/ContactsEndpointMapper.cs
touch ContactOpenApi/ContactDbContext.cs


touch ContactOpenApi/BearerSecuritySchemeTransformer.cs
touch ContactOpenApi/BearerOperationTransformer.cs
```

```bash
dotnet run --project ContactOpenApi/ContactOpenApi.csproj --launch-profile http

# http://localhost:5037/openapi/v1.json
# http://localhost:5037/scalar/


dotnet user-secrets init
dotnet user-jwts create
#dotnet user-jwts create --issuer "MyLocalIdentityServer" --audience "MySecureBackendApi" --role "Admin"
dotnet user-secrets set "Authentication:JwtToken" "PASTE_YOUR_COPIED_JWT_HERE"


dotnet user-secrets set "Authentication:JwtSigningKey" "SuperSecretLocalDevelopmentSigningKey2026!"

# 3. Define the trusted local issuer name
dotnet user-secrets set "Authentication:Issuer" "MyLocalIdentityServer"

# 4. Define the target audience matching your local web API
dotnet user-secrets set "Authentication:Audience" "MySecureBackendApi"
```
