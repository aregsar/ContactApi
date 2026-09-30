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

## Add logging levels in settings

```json

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
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
  },
  "EntityFramework": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  }
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=contacts.db"
  },
  "AllowedHosts": "*"
}


```

## Add the Contacts API

```bash
touch ContactEfCoreLogging/Contact.cs
touch ContactEfCoreLogging/ContactsEndpointMapper.cs
touch ContactEfCoreLogging/ContactDbContext.cs
```

Contact.cs

```cs

public record ContactResource(int Id, string FirstName, string? LastName, string Email);
public record CreateContactRequestData(string FirstName, string? LastName, string Email);
public record UpdateContactRequestData(string FirstName, string? LastName, string Email);
public record PatchContactRequestData(string? FirstName, string? LastName, string? Email);
public class Contact
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
}
```

ContactDbContext.cs

```cs
using Microsoft.EntityFrameworkCore;

class ContactDbContext : DbContext
{
    public ContactDbContext(DbContextOptions<ContactDbContext> options)
        : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();
}
```

ContactsEndpointMapper.cs

```cs

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public static class ContactsEndpointMapper
{
    private class ContactsEndpoint { };

    public static void Map(WebApplication app)
    {
        var contacts = app.MapGroup("/contacts").WithTags("Contacts");

        contacts.MapGet("/list", List).WithName("Contacts.List").WithSummary("");
        contacts.MapGet("/get/{id}", Get).WithName("Contacts.Get").WithSummary("");
        contacts.MapPost("/post", Post).WithName("Contacts.Post").WithSummary("");
        contacts.MapPut("/put/{id}", Put).WithName("Contacts.Put").WithSummary("");
        contacts.MapPatch("/patch/{id}", Patch).WithName("Contacts.Patch").WithSummary("");
        contacts.MapDelete("/delete/{id}", Delete).WithName("Contacts.Delete").WithSummary("");
    }

    static async Task<Ok<ContactResource[]>> List(ContactDbContext db,
                                                  CancellationToken token,
                                                  ILogger<ContactsEndpoint> logger)
    {
        return TypedResults.Ok(await db.Contacts
                                        .Select(contact => new ContactResource(contact.Id,
                                                                                contact.FirstName,
                                                                                contact.LastName,
                                                                                contact.Email))
                                        .ToArrayAsync(token));
    }

    static async Task<Results<Ok<ContactResource>, NotFound>> Get(int id,
                                                                    ContactDbContext db,
                                                                    CancellationToken token,
                                                                    ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id, token);

        return contact is not null
                ? TypedResults.Ok(new ContactResource(contact.Id, contact.FirstName, contact.LastName, contact.Email))
                : TypedResults.NotFound();
    }

    static async Task<CreatedAtRoute<ContactResource>> Post(CreateContactRequestData contactData,
                                                        ContactDbContext db,
                                                        CancellationToken token,
                                                        ILogger<ContactsEndpoint> logger)
    {
        var contact = new Contact
        {
            FirstName = contactData.FirstName,
            LastName = contactData.LastName,
            Email = contactData.Email,
        };

        db.Contacts.Add(contact);
        await db.SaveChangesAsync(token);

        var contactResource = new ContactResource(contact.Id, contact.FirstName, contact.LastName, contact.Email);

        return TypedResults.CreatedAtRoute(contactResource, "Contacts.Get", new { id = contactResource.Id });
    }

    static async Task<Results<NotFound, NoContent>> Put(int id,
                                                        UpdateContactRequestData contactData,
                                                        ContactDbContext db,
                                                        CancellationToken token,
                                                        ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id);

        if (contact is null)
        {
            return TypedResults.NotFound();
        }

        contact.FirstName = contactData.FirstName;
        contact.LastName = contactData.LastName;
        contact.Email = contactData.Email;

        await db.SaveChangesAsync(token);

        return TypedResults.NoContent();
    }

    static async Task<Results<NotFound, NoContent>> Patch(int id,
                                                            PatchContactRequestData contactData,
                                                            ContactDbContext db,
                                                            CancellationToken token,
                                                            ILogger<ContactsEndpoint> logger)
    {
        var contact = await db.Contacts.FindAsync(id, token);

        if (contact is null)
        {
            return TypedResults.NotFound();
        }

        //assumes null property value skips the patching so can not patch a field to null unless we replace the
        //properties of PatchContactRequestData with a Dictionary<string, System.Text.Json.JsonElement>? PropertyDict
        //Or use a JsonPatchDocument<Contact> instead of PatchContactRequestData
        contact.FirstName = contactData.FirstName ?? contact.FirstName;
        contact.LastName = contactData.LastName ?? contact.LastName;
        contact.Email = contactData.Email ?? contact.Email;
        // if (contactData.FirstName is not null) contact.FirstName = contactData.FirstName;
        // if (contactData.LastName is not null) contact.LastName = contactData.LastName;
        // if (contactData.Email is not null) contact.Email = contactData.Email;

        await db.SaveChangesAsync(token);

        return TypedResults.NoContent();
    }


    static async Task<Results<NotFound, NoContent>> Delete(int id,
                                                            ContactDbContext db,
                                                            CancellationToken token,
                                                            ILogger<ContactsEndpoint> logger)
    {
        if (await db.Contacts.FindAsync(id, token) is Contact contact)
        {
            db.Contacts.Remove(contact);

            await db.SaveChangesAsync(token);

            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

}
```

## Add the OpenAPI services and middleware

Program.cs file:

```cs

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContactDbContext>(options =>
{
    options.UseInMemoryDatabase("Contacts");

    // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    // ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    // options.UseSqlite(connectionString);
    // options.UseNpgsql(connectionString);
    // options.UseMySql(connectionString);
    // options.UseSqlServer(connectionString);

    bool enableSensitiveLogging = builder.Configuration.GetValue<bool>("EntityFramework:EnableSensitiveDataLogging", false);
    bool enableDetailedErrors = builder.Configuration.GetValue<bool>("EntityFramework:EnableDetailedErrors", false);

    if (enableSensitiveLogging)
    {
        options.EnableSensitiveDataLogging();
    }

    if (enableDetailedErrors)
    {
        options.EnableDetailedErrors();
    }
});


builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // app.MapScalarApiReference(options =>
    // {
    //     options.WithTitle("Contacts API");
    // });
}

app.MapGet("/", () => "Hello World!");

ContactsEndpointMapper.Map(app);

app.Run();

```
