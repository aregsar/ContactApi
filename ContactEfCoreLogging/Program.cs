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
