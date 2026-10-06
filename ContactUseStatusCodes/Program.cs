var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages();

app.MapGet("/", () => "Hello World!");

app.Run();