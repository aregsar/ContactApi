var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();