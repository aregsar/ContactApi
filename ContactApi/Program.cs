var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    // Remove the Kestrel Server header when not in development mode
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
    });
}

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
