# ContactEfCoreLogging

## Configure EFCore Logging

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

///////////////////////////////////////////////////////////////////////////////
//Optional redaction services
builder.Logging.EnableRedaction();
//standard compliance rules (e.g., Erasing or HMAC)
builder.Services.AddRedaction(redactionBuilder =>
{
    // Define that anything categorized as "Sensitive" gets erased or masked
    redactionBuilder.SetRedactor<ErasingRedactor>(DataClassificationSet.FromKeyValue("Taxonomy", "Sensitive"));

    // Set a fallback redactor for unspecified categories
    redactionBuilder.SetFallbackRedactor<StarRedactor>();
});

builder.Services.Configure<ComplianceEFCoreLoggingOptions>(options =>
{
    options.EnableSensitiveDataLogging = isSensitiveLoggingEnabled;
});
builder.Services.AddSingleton<ILoggerProvider, ComplianceEFCoreLoggingForwarder>(sp =>
{
    // Resolve safe dependencies immediately during startup
    var redactorProvider = sp.GetRequiredService<IRedactorProvider>();
    var options = sp.GetRequiredService<IOptions<ComplianceEFCoreLoggingOptions>>();

    // Pass a lambda delegate () => sp.GetRequiredService<T>() to satisfy the Func<ILoggerFactory> parameter.
    // This breaks the loop because ILoggerFactory is only looked up when a log is written.
    return new ComplianceEFCoreLoggingForwarder(
        () => sp.GetRequiredService<ILoggerFactory>(),
        redactorProvider,
        options
    );
});
//
//This is done in appsettings.json instead of in code:
// "Microsoft.EntityFrameworkCore.Database.Command": "None",
// "Microsoft.EntityFrameworkCore.Database.Command.Redacted": "Information",
//
// Mute the original raw EF Core logger so it doesn't leak raw credentials
// to your sinks alongside your redacted ones
//builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
// Allow your custom redacted namespace to output normally
//builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command.Redacted", LogLevel.Information);
///////////////////////////////////////////////////////////////////////////////

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

### Redacting

```cs
//intercept logging pipeline message, redact and forward to logging pipeline so all registered logging providers can log the redacted output
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;

public class ComplianceEFCoreLoggingOptions
{
    public bool EnableSensitiveDataLogging { get; set; }
}

public class ComplianceEFCoreLoggingForwarder : ILoggerProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRedactorProvider _redactorProvider;
    private readonly ComplianceEFCoreLoggingOptions _options; // Added

    public ComplianceEFCoreLoggingForwarder(
        Func<ILoggerFactory> loggerFactoryProvider,
        IRedactorProvider redactorProvider,
        IOptions<ComplianceEFCoreLoggingOptions> options)
    {
        _loggerFactoryProvider = loggerFactoryProvider;
        _redactorProvider = redactorProvider;
        _options = options.Value;
    }


    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName == "Microsoft.EntityFrameworkCore.Database.Command")
        {
            // Invoke the delegate only when needed
            var loggerFactory = _loggerFactoryProvider();
            // Create a safe target logger that distributes to ALL providers
            // Using a distinct category name prevents infinite recursion loops
            var targetLogger = loggerFactory.CreateLogger("Microsoft.EntityFrameworkCore.Database.Command.Redacted");
             // Pass options down to the internal forwarder
            return new RedactingForwarder(targetLogger, _redactorProvider, _options);
        }
        // Return a dummy logger for other EF categories so we don't interfere
        return NullLogger.Instance;
    }

    public void Dispose() { }

    private class RedactingForwarder : ILogger
    {
        private readonly ILogger _target;
        private readonly Redactor _redactor;
        private readonly ComplianceEFCoreLoggingOptions _options; // Added
        private static readonly Regex ParameterRegex = new(@"(@\w*(?:password|secret|ssn)\w*)\s*=\s*'([^']*)'", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public RedactingForwarder(ILogger target, IRedactorProvider redactorProvider, ComplianceEFCoreLoggingOptions options)
        {
            _target = target;
            _options = options;
            _redactor = redactorProvider.GetRedactor(DataClassificationSet.FromKeyValue("Taxonomy", "Sensitive"));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _target.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _target.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);

            // Conditional execution: Redact ONLY if EnableSensitiveDataLogging is set to true
            if (_options.EnableSensitiveDataLogging && !string.IsNullOrEmpty(message))
            {
                // Redact using .NET 10 Engine
                message = ParameterRegex.Replace(message, match =>
                {
                    string paramName = match.Groups[1].Value;
                    string rawValue = match.Groups[2].Value;
                    return $"{paramName} = '{_redactor.Redact(rawValue)}'";
                });
            }

            _target.Log(logLevel, eventId, exception, "{Message}", message);
        }
    }
}
```
