using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    //In non development environments we only rely on OpenTelemetry Logging provider
    builder.Logging.ClearProviders();
}

builder.AddOpenTelemetry();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/trace", () =>
{
    using Activity? activity = BuilderExtensions.MyActivitySource.StartActivity("ProcessOrderEndpoint");
    activity?.SetTag("id", 1);
    activity?.SetStatus(ActivityStatusCode.Error, "User not found");
});

app.Run();