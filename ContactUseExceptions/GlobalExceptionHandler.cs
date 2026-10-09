
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env
    ) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        int statusCode = exception switch
        {
            // BadHttpRequestException badRequestEx => badRequestEx.StatusCode,
            // NotImplementedException => StatusCodes.Status501NotImplemented,
            // UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        ProblemDetailsContext context = new()
        {
            HttpContext = httpContext,
            Exception = exception
        };

        //Use the existing context.ProblemDetails. Do not create a new ProblemDetails object.
        context.ProblemDetails.Status = statusCode;
        context.ProblemDetails.Title = "An error occurred while processing your request.";

        if (env.IsDevelopment())
        {
            var exceptionFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
            var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
            //override the Title in development mode
            context.ProblemDetails.Title = GetTypeDisplayName(exception.GetType());
            context.ProblemDetails.Detail = exception.Message; //"error";

            ////context.ProblemDetails.Extensions ??= new Dictionary<string, object?>(StringComparer.Ordinal);

            //build exception data that matches the UseDeveloperExceptionPage exception data
            var exceptionData = new
            {
                details = exception.ToString(),//exception.ToString() provides most comprehensive information
                headers = httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray()),
                path = exceptionFeature?.Path ?? httpContext.Request.Path.Value,
                endpoint = endpointFeature?.Endpoint?.DisplayName ?? "Unknown",
                routeValues = exceptionFeature?.RouteValues ?? new RouteValueDictionary()
            };

            context.ProblemDetails.Extensions.TryAdd("exception", exceptionData);

            //context.ProblemDetails.Extensions.TryAdd("exception", exception.ToString());
            //context.ProblemDetails.Extensions.TryAdd("exception", exception.Message);


        }

        return await problemDetailsService.TryWriteAsync(context);
    }

    //Gets Fully Qualified Friendly Name
    private static string GetTypeDisplayName(Type type)
    {
        if (!type.IsGenericType)
            return type.FullName ?? type.Name;

        var genericArguments = type.GetGenericArguments();
        var typeName = type.Name[..type.Name.IndexOf('`')];
        var argumentNames = string.Join(", ", genericArguments.Select(GetTypeDisplayName));

        return $"{type.Namespace}.{typeName}<{argumentNames}>";
    }
}