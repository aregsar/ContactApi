# ContactValidationExceptions

## Configure Problem Details Error response for unhandled validation exceptions

In the previous GLOBAL_EXCEPTION_HANDLING post we learned how to add global exception handing middleware that executes a pipeline of exception handlers.

In that post we added a single global exception handler to the pipeline to handle unhandled application exceptions.

In this post we will add another exception handler to the pipeline that will have the opportunity to intercept thrown exceptions and handle them if they are of the type of domain validation error.

If the exception thrown is not a validation error then the exception middleware will give the global exception handler a chance to handle the exception as a fallback.

The validation exception handler will return a validation problem detail json response.

A validation problem details response is a problem details response with additional validation error information included problem details extensions dictionary.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactValidationExceptions -o ContactValidationExceptions
cd ContactValidationExceptions
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactValidationExceptions" >> README.md
```

### Create the project and add to solution

Create a basic project boilerplate and add it to the solution (.slnx) file:

```bash
dotnet new web -o ContactValidationExceptions
dotnet sln ContactApi.slnx add ContactValidationExceptions/ContactValidationExceptions.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

## Adding the global exception handling middleware and handler

So as a starting point let us quickly add the code that we added in the GLOBAL_EXCEPTION_HANDLING post as a starting point for this post.

So lets start by adding the exception handing logic file from that post:

```bash
touch ContactUseExceptions/GlobalExceptionHandler.cs
```

Here is the code for the file from that post:

```cs
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
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            NotImplementedException => StatusCodes.Status501NotImplemented,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
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
            context.ProblemDetails.Detail = exception.Message;

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
```

Finally lets add the changes to the Program.cs file from that post that adds the exception handling middleware, enables the required problem details service and adds GlobalExceptionHandler to the exception handling pipeline:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();
```

The builder.Services.AddProblemDetails() adds the IProblemDetailsWriter implementation that the GlobalExceptionHandler uses to write the problem details response to the service container.

The app.UseExceptionHandler() call adds the exception handling middleware to the request processing pipeline.

The builder.Services.AddExceptionHandler<GlobalExceptionHandler> adds GlobalExceptionHandler to the exception handlers pipeline that the  middleware added by UseExceptionHandler calls.

## IProblemDetailsWriter recap

The GLOBAL_EXCEPTION_HANDLING post describes how the IProblemDetailsWriter works but I will provide brief description here.

The Validation Exception handler  writes a problem details error response using the IProblemDetailsWriter implementation registered with the service container by the AddProblemDetails.

The IProblemDetailsWriter implementation will serialize a problem details context object to the output stream.

This object is created by the exception handler that calls IProblemDetailsWriter TryWrite and passed to the TryWrite method to be serialized.

The IProblemDetailsWriter TryWrite method also triggers a global pre serialization problem details interceptor, if the interceptor is provided to AddProblemDetails.

So far we hve not provided this interceptor but we will learn about this interceptor in another post.

## Adding Validation Exception classes

Before creating our validation exception handler we will create the validation exception class that it should handle.

We will start by creating a base validation exception class that our domain specific validaion exceptions will derive from:
