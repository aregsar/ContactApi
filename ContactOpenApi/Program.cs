using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContactDbContext>(opt => opt.UseInMemoryDatabase("Contacts"));
builder.Services.AddOpenApi();

// builder.Services.AddAuthentication();
// builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    // app.MapScalarApiReference(options =>
    // {
    //     // Changes the default /scalar URL to /docs/v1
    //     options.WithEndpointPrefix("/docs/{documentName}");
    // });
}

// app.UseAuthentication();
// app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
ContactsEndpointMapper.Map(app);

app.Run();
