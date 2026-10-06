using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();
//app.UseStatusCodePages();
app.UseStatusCodePages(StatusCodePagesHandler.WriteProblemDetailsAsync);
//app.UseStatusCodePages(async context => await StatusCodePagesHandler.WriteProblemDetailsAsync(context));
// app.UseStatusCodePages(async context =>
// {
//     var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
//     var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
//     {
//         Status = context.HttpContext.Response.StatusCode,
//         Title = "Not Found",
//         //Instance = context.HttpContext.Request.Path
//     };
//     //var traceId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
//     //problemDetails.Extensions["traceId"] = traceId;
//     await problemDetailsService.WriteAsync(new ProblemDetailsContext
//     {
//         HttpContext = context.HttpContext,
//         ProblemDetails = problemDetails
//     });
// });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World! ok");

app.Run();