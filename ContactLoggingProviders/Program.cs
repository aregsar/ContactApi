var builder = WebApplication.CreateBuilder(args);
builder.AddLoggingProviders();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
