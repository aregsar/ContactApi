# ContactProblemDetails

## Configure Problem Details writer service and global Problem Details serialization interceptor

A problemdetails http response is written using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

Before IProblemDetailsWriter implementation serializes problemdetails object it can call a global pre-serialization interceptor if an interceptor callback is registered.

The callback can modify the problemdetails object before it is serialized to the output stream.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactProblemDetails -o ContactProblemDetails
cd ContactProblemDetails
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactProblemDetails" >> README.md
```

### Create the project and add to solution

Create a basic project boilerplate and add it to the solution (.slnx) file:

```bash
dotnet new web -o ContactProblemDetails
dotnet sln ContactApi.slnx add ContactProblemDetails/ContactProblemDetails.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

## Adding files from previous posts

```bash
touch ContactProblemDetails/CustomValidationException.cs
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

```bash
touch ContactProblemDetails/BadRequestException.cs
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

```bash
touch ContactProblemDetails/GlobalExceptionHandler.cs
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

```bash
touch ContactProblemDetails/ValidationExceptionHandler.cs
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

## Updating  Program.cs  to add exception handling

Update the Program.cs with the code from the ValidationExceptionHandling and UseStatusCodePages posts.

The code has an additional call to  app.UseStatusCodePages after calling app.UseExceptionHandler().

The call adds the UseStatusCodePages middleware that we learned about in the UseStatusCodePages post.

> Since middleware execution order matters, the app.UseStatusCodePages() is called right after app.UseExceptionHandler() to make sure that the exception handling middleware will catch any exceptions thrown from the UseStatusCodePages middleware.

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapGet("/key", () =>
{
    throw new KeyNotFoundException();
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

## Adding the problem details interception code - Customizing AddProblemDetails

The IProblemServiceDetails.TryWriteAsync call prior to serializing the ProblemDetailsContext object passed to it, can call a global interceptor callback that can modify the ProblemDetailsContext object prior to serialization.

the builder.Services.AddProblemDetails call in Program.cs allows us to add this global interceptor callback so that problemDetailsService.TryWriteAsync can call it passing it the problem details context object before serializing it to the output.

## Adding a global pre serialization Interceptor callback to AddProblemDetails

Any middleware that writes ProblemDetails output, creates the ProblemDetailsContext and sets its ProblemDetails property, and passes the ProblemDetailsContext to TryWriteAsync() to serialize as json to the output.

Before TryWriteAsync() serializes the ProblemDetailsContext it checks if there is a response interceptor callback method registered.

If it does not find a callback which is the case at the moment, TryWriteAsync() will just go ahead and serialize the context to the response and add the Content-Type: application/problem+json header.

However if WriteJsonAsync finds the callback method it first calls the callback passing the callback the ProblemDetailsContext object that is to be serialized.

The callback code can make modifications to the context and after it returns to TryWriteAsync, the updated context object will be serialized to the output by TryWriteAsync.

Lets demonstrate how the ProblemDetails pre serialization callback can enrich the ProblemDetails object serialized by the UseStatusCodePages and Our Exception handlers with additional data.

When the exception handler middleware or the status code pages middleware call problemDetailsService.WriteAsync to write the ProblemDetails output, we have an opportunity to add a hook that can intercept the writing and customize the problem details output.

We do this by providing an options lambda to the AddProblemDetails call. The lambda will be called back when problemDetailsService.WriteAsync is called and can modify the ProblemDetailsContext before it is serialized to the output stream. If we add Minimal API input validation then this lambda will also run when the ASP.NET frameworks input validation exception handler runs so that our problem details basic output format is consistant no matter how exceptions are handlerd.

Lets add this now setting our format handling logic to the CustomizeProblemDetails callback of the options passed to AddProblemDetails:

The updated file looks like below:

Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    //options.CustomizeProblemDetails is set to the interceptor callback.
    //The saved CustomizeProblemDetails is called by IProblemDetailsService.TryWriteAsync
    //passing it the ProblemDetailsContext context before the context is serialized
    options.CustomizeProblemDetails = context =>
    {
        //set the ProblemDetails status to the HttpContext response status prior to serialization
        //some error handlers might have missed setting context.ProblemDetails.Status so
        //setting here ensures that it will always be set regardless of how the error is handled
        //as long as all error handlers use IProblemDetailsService.TryWriteAsync or
        //IProblemDetailsService.WriteAsync to serialize the problem details context
        context.ProblemDetails.Status = context.HttpContext.Response.StatusCode;

        context.ProblemDetails.Title ??= context.HttpContext.Response.StatusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            _ => "An unexpected error occurred while processing your request"
        };

        //instance is only added in the global interceptor. not in any error handlers
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path} {DateTime.UtcNow}";

        //Add to context.ProblemDetails.Extensions
        IHttpActivityFeature? activityFeature = context.HttpContext.Features.Get<IHttpActivityFeature>();

        context.ProblemDetails.Extensions.TryAdd("timestamp", DateTimeOffset.UtcNow);

        //we try adding the traceId extension property in case it wasn't added by default by the framework
        context.ProblemDetails.Extensions.TryAdd("traceId", activityFeature?.Activity?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("activityId", activityFeature?.Activity?.Id ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("rootId", activityFeature?.Activity?.RootId ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("traceIdentifier", context.HttpContext.TraceIdentifier);

    };
});

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapGet("/key", () =>
{
    throw new KeyNotFoundException();
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

With this update we are passing a problem detail options delegate to the AddProblemDetails method and inside the body we assign to options.CustomizeProblemDetails a problem details context delegate.

In the problem details context delegate the first thing we do is set the context.ProblemDetails.Status to context.HttpContext.Response.StatusCode to make sure problem details has the final http context status just before it is serialized to the response.

Then we add various additional useful properties to the problem details object if not already set.

> In the interceptor we only update problem details related properties only. Any exception data is added by the exception handlers.

Here we used the global interceptor hook to make final modifications to the context.ProblemDetails object that will be serialized as json output.

First thing we do in here is to make sure that the status set on the HttpContext is set to the ProblemDetails status.

This ensures that the status between them is in sync even if some custom exception handler changed the HttpContext status but failed to update the ProblemDetails it wrote to the output stream using IProblemDetailsService.

Then we set a title based on the status code.

Finally we add some basic tracing data to the problem details output that we can use to track and debug production errors.

The context.ProblemDetails.Extensions["traceId"] is normally added by default by the asp.net framework when  problemDetailsService.TryWriteAsync is called to in error handlers, however we try adding it here again just to make sure it is available.

If it already exists the TryAdd will not replace it.

Then we add a number of other identifiers that may be helpfull in debugging production requests.

One thing to note is that we use context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id to access the current activity id instead of using Microsoft.AspNetCore.Diagnostics.Activity.Current?.Id. This is done to avoid threading and async flow issues from causing Activity.Current to be null. In some scrnarions where HttpContext is not available the Microsoft.AspNetCore.Diagnostics.Activity.Current AsyncLocal property can be used.

> Note that we did not add or modify any exception handling data. This is good practice since we dont want exception data from different exception handlers that we might add to the application to be changed. The exception handlers should be responsable for setting exception data.

## Wrapping AddProblemDetails configuration in a static method

We need to setup the Global problem details serialization interceptor that intercepts calls to problemDetailsService.TryWriteAsync(problemdetails) and customizes the problem details context object by adding common data in one global location before it gets serialized.

The interceptor also allows us to set the http context status to the problem details object status property to make sure they are in sync. So even if we you forget to set it in the exception handler there is one global location that it can be set.

```bash
touch ContactProblemDetails/GlobalProblemDetailsInterceptor.cs
```

Now we need to first add a GlobalProblemDetailsInterceptor class with a static PreSerializationHook callback method to the file.

Then move the inline callback code set to the options.CustomizeProblemDetails in the AddProblemDetails call in Program.cs into this static method.

Then move the inline callback code set to the options.CustomizeProblemDetails from the AddProblemDetails call into this static method.

Here is the code in GlobalProblemDetailsInterceptor.cs

```cs
using Microsoft.AspNetCore.Http.Features;

public class GlobalProblemDetailsInterceptor
{
    public static void PreSerializationCallback(ProblemDetailsContext context)
    {
        //set the ProblemDetails status to the HttpContext response status prior to serialization
        context.ProblemDetails.Status = context.HttpContext.Response.StatusCode;

        context.ProblemDetails.Title ??= context.HttpContext.Response.StatusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            _ => "An unexpected error occurred while processing your request"
        };

        //instance is only added in the global interceptor. not in any error handlers
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path} {DateTime.UtcNow}";


        //Add to context.ProblemDetails.Extensions
        IHttpActivityFeature? activityFeature = context.HttpContext.Features.Get<IHttpActivityFeature>();

        context.ProblemDetails.Extensions.TryAdd("timestamp", DateTimeOffset.UtcNow);

        //we try adding the traceId extension property in case it wasn't added by default by the framework
        context.ProblemDetails.Extensions.TryAdd("traceId", activityFeature?.Activity?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("activityId", activityFeature?.Activity?.Id ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("rootId", activityFeature?.Activity?.RootId ?? context.HttpContext.TraceIdentifier);

        context.ProblemDetails.Extensions.TryAdd("traceIdentifier", context.HttpContext.TraceIdentifier);

    }
}
```

Now we can update the call to AddProblemDetails in Program.cs to use the GlobalProblemDetailsInterceptor.PreSerializationCallback:

Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);

//CustomizeProblemDetails is set to the interceptor callback
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = GlobalProblemDetailsInterceptor.PreSerializationCallback);

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.MapGet("/key", () =>
{
    throw new KeyNotFoundException();
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

problemDetailsService.TryWriteAsync calls this callback.

The callback updates the context.ProblemDetails.

The modified context.ProblemDetails will be serialized to the http response output stream once the callback returns back to problemDetailsService.TryWriteAsync.

## Testing The problem details response

TODO: Add .http and xunit tests
