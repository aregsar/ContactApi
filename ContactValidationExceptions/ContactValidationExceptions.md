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

## Adding the production profile

Lets update the properties/launchSettings.json to add a production profile enabling us to run the application in production mode.

This will help with testing the exception handling output when running in development and production modes.

Update the properties/launchSettings.json.

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5095",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "http-prod": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5095",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7192;http://localhost:5095",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

The http-prod was added to the default launchSettings file.

## Adding Validation Exception classes

Before creating our validation exception handler we will create the validation exception classes that it should handle.

Lets first create a CustomValidationException abstract base validation class that all our custom domain specific application exceptions will derive from.

It provides basic properties that will be common to all custom application exceptions.

The abstract class will allow each of the derived classes to specify their own http status code value by overriding the abstract StatusCode property.

Same goes for the for the abstract StatusCodeDescription property.

Create the CustomValidationException file as a base class for all our derived validation exceptions.

```bash
touch src/Api/Contax.Api/CustomValidationException.cs
```

```cs
public abstract class CustomValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public abstract int StatusCode { get; }

    public abstract string StatusCodeDescription { get; }

    protected CustomValidationException(string baseMessage, IDictionary<string, string[]> errors)
        : base(baseMessage)
    {
        Errors = errors;
    }
}
```

Lets create a BadRequestException exception derived from CustomValidationException.

It sets the StatusCode and StatusCodeDescription and can except an error message and error dictionary.

Create the derived BadRequestException file:

```bash
touch src/Api/Contax.Api/BadRequestException.cs
```

```cs

public class BadRequestException : CustomValidationException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;

    public override string StatusCodeDescription => "400 Bad Request";

    public BadRequestException(string message, IDictionary<string, string[]> errors)
        : base(message, errors) { }
}
```

## Adding the global exception handling middleware and handler

So as a starting point let us quickly add the code that we added in the GLOBAL_EXCEPTION_HANDLING post as a starting point for this post.

Add a new GlobalExceptionHandler.cs file:

```bash
touch ContactValidationExceptions/GlobalExceptionHandler.cs
```

Add the content of the GlobalExceptionHandler file from the GLOBAL_EXCEPTION_HANDLING post:

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

## IProblemDetailsWriter recap

The GLOBAL_EXCEPTION_HANDLING post describes how the IProblemDetailsWriter works but I will provide brief description here as well.

The GlobalExceptionHandler writes a problem details error response using the constructor injected IProblemDetailsWriter implementation.

It creates a ProblemDetailsContext object then calls IProblemDetailsWriter.TryWrite passing in the ProblemDetailsContext object.

The IProblemDetailsWriter implementation will serialize the ProblemDetailsContext object to the output stream.

The IProblemDetailsWriter implementation will be added to the service container by calling AddProblemDetails when we update our Program.cs later in this post.

## Creating the ValidationExceptionHandler

Now that we have out base exception class and custom BadRequestException exception class
lets create a ValidationExceptionHandler exception handler.

The ValidationExceptionHandler exception handler that will respond to any exception handlers that are derived from CustomValidationException:

```bash
touch src/Api/Contax.Api/ValidationExceptionHandler.cs
```

```cs
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;



public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger,
                IProblemDetailsService problemDetailsService) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CustomValidationException validationException)
        {
            return false;
        }

        logger.LogError(validationException, "A validation exception occurred: {Message}", validationException.Message);

        int statusCode = validationException.StatusCode;

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new HttpValidationProblemDetails(validationException.Errors)
        {
            Status = statusCode,
            Title = exception.GetType().Name,
            Detail = validationException.Message,
            Instance = httpContext.Request.Path
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}
```

If the exception is not derived from CustomValidationException then the handler returns false allowing the next handler in the pipeline to handle the exception.

Again the handler uses the IProblemDetailsService implementation to seialize the ProblemDetailsContext to the output.

## Updating Program.cs with exception handling logic

Before we add out validation exception handler code we will update our Program.cs file with exception handling logic that we introduced in the GLOBAL_EXCEPTION_HANDLING post.

This way we can test the code
Update Program.cs with the following code.

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

app.MapGet("/key", () =>
{
    throw new KeyNotFoundException();
});

app.Run();
```

The builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt; adds GlobalExceptionHandler to the exception handlers pipeline.

The app.UseExceptionHandler() call adds the exception handling middleware to the request processing pipeline that calls the GlobalExceptionHandler to handle the exception.

The builder.Services.AddProblemDetails() adds the IProblemDetailsWriter implementation that the GlobalExceptionHandler uses to write the problem details response to the service container.

## adding the .http request file

Lets add a .http file that we can use to test our exception handling pipeline.

Add the .http file to the project root:

```bash
touch ContactValidationExceptions/ContactValidationExceptions.http
```

Add a HTTP GET request to the .http file:

```http
@baseUrl = http://localhost:5095

### Get Root URL
GET {{baseUrl}}/error
Accept: application/json
```

## Testing the Global Exception Handler

Before we add the Validation Exception Handling code lets test the Global Exception Handler that we already have in the pipeline

### Adding ValidationExceptionHandler to the pipeline

```cs

using Contax.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapGet("/bad", () =>
{
     throw new BadRequestException("Invalid email.",
                                     new Dictionary<string, string[]> {
                                        { "email", ["email can not be empty."] }
                                     });
});

app.Run();
```

We added builder.Services.AddExceptionHandler<ValidationExceptionHandler>() before adding GlobalExceptionHandler. The order matters since the handler are called top to bottom.

## Testing the Validation Exception Handler
