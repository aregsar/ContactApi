using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContactDbContext>(opt => opt.UseInMemoryDatabase("Contacts"));

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
