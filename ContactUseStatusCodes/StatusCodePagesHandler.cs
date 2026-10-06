using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public static class StatusCodePagesHandler
{
    public static async Task WriteProblemDetailsAsync(StatusCodeContext context)
    {
        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        var problemDetails = new ProblemDetails
        {
            Status = context.HttpContext.Response.StatusCode,
            Title = "Not Found",
            //Instance = context.HttpContext.Request.Path
        };

        // var traceId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        // problemDetails.Extensions["traceId"] = traceId;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails = problemDetails
        });
    }
}