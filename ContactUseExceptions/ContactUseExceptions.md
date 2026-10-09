# ContactUseExceptions

## Returning Problem Details Error response for unhandled exceptions

When running minimal apis we want to return a consistent  error response when our application throws an exception that is not handled.

We can leverage the ProblemDetails RFC to return a  ProblemDetails json response that all clients can handle in consistent manner.

By default in non development mode the asp.net request pipeline will return a generic error response for any unhandled exceptions.

In development mode the asp.net framework under the hood adds the UseDeveloperExceptionPage middleware to the pipeline that adds additional debugging info like the stack trace.

In this article I will show you how the asp.net UseExceptions middleware and the UseProblemDetails service work together to handle unhandled exceptions and return standard problem details responses.

MOVE THIS:
so that our unhandled exception go through a single unified middleware path in all environments.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactUseExceptions -o ContactUseExceptions
cd ContactUseExceptions
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactUseStatusCodes" >> README.md
```

### Create the project and add to solution

Create a basic project boilerplate and add it to the solution (.slnx) file:

```bash
dotnet new web -o ContactUseExceptions
dotnet sln ContactApi.slnx add ContactUseExceptions/ContactUseExceptions.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

Lets map a new error endpoint that triggers an unhandled exception from the request handler.

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();
```

### Adding a .http file for making API requests

Before you add the .http file make sure your editor is configured to support .http files.

> Some editors like Visual Studio or Rider have built in support. VSCode editor needs the `REST Client for Visual Studio Code` extension.

Add the .http file to the project root:

```bash
touch ContactUseExceptions/ContactUseExceptions.http
```

Add a HTTP GET request to the .http file:

```http
@baseUrl = http://localhost:5095

### Get Root URL
GET {{baseUrl}}/error
Accept: application/json
```

Replace the `@baseUrl` port number  with the randomly generated port number of the `applicationUrl` setting in the `http` profile of your projects `Properties/launchSettings.json` file.

## Adding a Production launch Settings profile

Before we make a request to this endpoint to see how unhandled exceptions are handled by default, lets add new profile to our launchSettings.json file so that we can easily run our code in non development mode.

Here is wh Properties/launchSettings.json looks like currently:

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

We can copy the `http` profile and change the name to `http.prod` and paste it below the `http` profile.

Then we can change the value of the `ASPNETCORE_ENVIRONMENT` property of this new `http-prod` profile to `"Production"`.

The final resulting file will look like:

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

> The port numbers in your settings file may be different

Now we can easily run the app in development and production mode by using the appropriate profile flag.

With this we can see how the default exception responses differ in development vs non development modes.

### Default Exception handling response in development

Lets run the development profile

```bash
dotnet run --project ContactUseExceptions/ContactUseExceptions.csproj --launch-profile http
```

Make request to <http://localhost:5095/error> using the .http file or curl:

```bash
curl -i -H "Connection: close" http://localhost:5095/error
```

The response look like:

```http
HTTP/1.1 500 Internal Server Error
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Mon, 17 Aug 2026 19:22:26 GMT
Server: Kestrel
Transfer-Encoding: chunked

System.Exception: error
   at Program.<>c.<<Main>$>b__0_1() in /Users/aregsarkissian/RiderProjects/ContactUseExceptions/ContactUseExceptions/Program.cs:line 13
   at lambda_method2(Closure, Object, HttpContext)
   at Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

HEADERS
=======
Accept: application/json
Connection: close
Host: localhost:5095
User-Agent: vscode-restclient
Accept-Encoding: gzip, deflate
```

By default in development mode the asp.net framework under the hood adds the UseDeveloperExceptionPage middleware to the pipeline that writes debugging info like the stack trace.

The response content type is:  Content-Type: text/plain; charset=utf-8

In addition in the console log we see :

```bash
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.Exception: error
         at Program.<>c.<<Main>$>b__0_1() in /Users/aregsarkissian/RiderProjects/ContactApi/ContactUseExceptions/Program.cs:line 9
         at lambda_method2(Closure, Object, HttpContext)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

```

We can see the Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke call.

However the output is in plain text and not in problem details json format.

### Default Exception handling response in Production

Now lets run the production profile:

```bash
dotnet run --project ContactUseExceptions/ContactUseExceptions.csproj --launch-profile http-prod
```

Make the same request again an we will the different output:

```http
HTTP/1.1 500 Internal Server Error
Content-Length: 0
Connection: close
Date: Mon, 17 Aug 2026 20:01:05 GMT
Server: Kestrel
```

The response is  just a 500 status error without any content.

By default since we are not running in dev mode the pipeline does not display error information that attacker might use.

Since the UseDeveloperException page is not added by asp.net framework when the environment is not development, there is no error handling middlewar to display the stack trace.

### Changing the output of UseDeveloperExceptionPage to problem details

UseDeveloperExceptionPage will format its response as a problem details json format if the problem details serialization service can be resolved from the service container.

To add that dependancy we need to add the builder.Services.AddProblemDetails call to Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();
```

The AddProblemDetails call adds a IProblemDetailsWriter implementation to the service container.

If the UseDeveloperExceptionPage middleware finds that service in the container, it will use it to write a ProblemDetails response json content to the response stream.

If UseDeveloperExceptionPage cant find it, falls back on the generic status code content output that we saw earlier.

The UseDeveloperExceptionPage middleware calls the WriteAsyncJson method of the IProblemDetailsWriter passing it a ProblemDetailsContext object that WriteAsyncJson serializes to the output stream.

Lets run the development profile again:

```bash
dotnet run --project ContactUseExceptions/ContactUseExceptions.csproj --launch-profile http
```

Now click on the send request button in the .http file to send the the request again.

The response should look like:

```http
HTTP/1.1 500 Internal Server Error
Connection: close
Content-Type: application/problem+json
Date: Thu, 08 Oct 2026 21:34:40 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "System.Exception",
  "status": 500,
  "detail": "error",
  "exception": {
    "details": "System.Exception: error\n   at Program.<>c.<<Main>$>b__0_1() in /Users/aregsarkissian/RiderProjects/ContactApi/ContactUseExceptions/Program.cs:line 11\n   at lambda_method2(Closure, Object, HttpContext)\n   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)",
    "headers": {
      "Accept": [
        "application/json"
      ],
      "Connection": [
        "close"
      ],
      "Host": [
        "localhost:5292"
      ],
      "User-Agent": [
        "vscode-restclient"
      ],
      "Accept-Encoding": [
        "gzip, deflate"
      ]
    },
    "path": "/error",
    "endpoint": "HTTP: GET /error",
    "routeValues": {}
  },
  "traceId": "00-080b544e6aa698f669bf3228632106ce-aaeb0e690a26da6c-00"
}
```

As we can see now we have a problem details json response in the body with the status and a traceId for the request.

The response content type is now:   Content-Type: application/problem+json

The the stack trace is written to the exception property.
The exception property is an extended problem details property in a property dictionary.

The console output still shows that the under the hood UseDeveloperExceptionPage middleware is still being invoked.

## Adding the Problem Details with the UseExceptionHandler middleware

So far we have Problem Details output in development mode but as we saw earlier there
was error content returned except status code in production environment.

This issue also applies to other non development environments.

The way we add problem details content is by adding the UseExceptionHandler.

The  UseExceptionHandler relies on the IProblemDetailsWriter implementation that writes the problem details response.

If we call UseExceptionHandler without calling AddProblemDetails the runtime will throw an exception when we try to run the application.

Since we already added AddProblemDetails for the UseDeveloperExceptionPage middleware we should be ready.

So lets add the UseExceptionHandler middleware to Program.cs

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();
```

Now lets run the production profile and see what we get:

```bash

dotnet run --project ContactUseExceptions/ContactUseExceptions.csproj --launch-profile http-prod
```

Make the same request again an we will now see the problem details output:

```http
HTTP/1.1 500 Internal Server Error
Connection: close
Content-Type: application/problem+json
Date: Thu, 08 Oct 2026 21:43:03 GMT
Server: Kestrel
Cache-Control: no-cache,no-store
Expires: -1
Pragma: no-cache
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "00-abc6f785c2bebf49d00f83fb66584254-49245057ad97cb59-00"
}
```

## UseExceptionHandler interaction with UseDeveloperExceptionPage

But what happens if we run the development profile with the UseExceptionHandler call.

Lets run the development profile again and see what we get now:

```bash

dotnet run --project ContactUseExceptions/ContactUseExceptions.csproj --launch-profile http
```

Make the same request again an we will now see the problem details output:

```http
HTTP/1.1 500 Internal Server Error
Connection: close
Content-Type: application/problem+json
Date: Thu, 08 Oct 2026 21:56:07 GMT
Server: Kestrel
Cache-Control: no-cache,no-store
Expires: -1
Pragma: no-cache
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "00-459ee45ee2f91337f348bb656350c9c7-aa8ff02e6aa8429e-00"
}
```

Now we see the same output as production.

That is because when we call UseExceptionHandler it overrides the UseDeveloperExceptionPage in the pipeline.

But we still need to see our stack trace when running in development mode, so how can we get the stack trace during development and but in production.

Well there are two approaches.

### Approach 1 - Using UseExceptionHandler only in non development environment

The simplest and easiest to implement approach is to only call UseExceptionHandler if we are running in development mode.

This way in development mode the under the hood UseDeveloperExceptionPage will not be overridden and will handle the exception and will include the stack trace in the response.

If we are running in non development mode we will call UseExceptionHandler which will override UseDeveloperExceptionPage and handle the exception without writing the stack trace to the response.

If you opt for this approach this Program.cs change is all you need:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    throw new Exception("error");
});

app.Run();
```

The UseExceptionHandler is now wrapped inside an environment check.

When running in any other environment other than development the UseExceptionHandler middleware will override the UseDeveloperExceptionPage middleware.

The problem with this approach is you now have two completely different middleware pipeline paths for development vs non development.

The second approach solves this problem but still allows stack trace information to be output when running in development mode.

The other benefit of the second approach is that it allows us to customize the exception response both for development and non development mode.

### Approach 2 - Always using UseExceptionHandler with a GlobalExceptionHandler for all environments

The second approach allows all unhandled exceptions to be handled by the UseExceptionHandler middleware while preserving providing additional information when running in development mode.

With this approach we simply always call UseExceptionHandler regardless of what environment the app is running in.

Then we write a custom global exception handler that will be executed by UseExceptionHandler instead of its default exception handling code.

In our custom handler we can check if we are running in development mode and add the stack trace and any other info we need to the output.

The benefit of this approach is there is only a single middleware code path for exception handling in our application regardless of the environment.

The added benefit of this approach is that we can completely customize the output instead of relying on the default UseExceptionHandler exception handling implementation.

In our custom implementation we can use the same underlying IProblemDetailsWriter implementation added by AddProblemDetail to write our problem details response.

> Using the IProblemDetailsWriter implementation is important because it allows our implementation to use the same global Problem Details output serilalization hook that all other framework handlers that use IProblemDetailsWriter have access to.

In the sections below we will see how to implement this approach

### Using UseExceptionHandler with a GlobalExceptionHandler

Adding a custom GlobalException handler to UseExceptionHandler middleware replaces the default exception handling implementation by the middleware.

The old way to add a custom global exception handler to UseExceptionHandler was to pass in a delegate argument that encapsulated the exception handling code.

The modern approach to add the custom global exception handler is to use the asp.net exception handler pipeline by adding an IExceptionHandler implementation to the service container.

We will implement the modern approach which will allow us to add additional specialized exception handlers in the future should we require.

The custom handler can also log exception details.

So lets start by adding a file for the exception handing logic:

```bash
touch ContactUseExceptions/GlobalExceptionHandler.cs
```

```cs

//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;



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
            //override the Title in development mode
            context.ProblemDetails.Title =  = GetTypeDisplayName(exception.GetType()),
            context.ProblemDetails.Detail = errorContext.Exception.Message;
            //context.ProblemDetails.Detail = "error"; //exception.Message;

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

            problemDetails.Extensions.TryAdd("exception", exceptionData);

            //context.ProblemDetails.Extensions.TryAdd("exception", exception.ToString());
            //context.ProblemDetails.Extensions.TryAdd("exception", exception.Message);


        }

        return await problemDetailsService.TryWriteAsync(context);

    }


    //GetFullyQualifiedFriendlyName
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

We can now register our GlobalExceptionHandler with the exception handler pipeline by simply calling builder.Services.AddExceptionHandler.

Any exception handler we add must implement IExceptionHandler.

The UseExceptionHandler will now retrieve the next added exception handler from the container and call that handlers TryHandleAsync method to handle the exception.

Since GlobalExceptionHandler is the only handler added, there is only one handler that will be called. If the handler returns false then UseExceptionHandler will run its own default internal fallback handler which is the internal handler we have seen before.

> Note that you can customize the fallback handler by providing the classic style exception handler delegate as an argument to UseExceptionHandler.

If a handler returns true, UseExceptionHandler stops looking for the next registered handler and continues its normal execution path.

If multiple handler are added, UseExceptionHandler goes through them starting with the first added handler one returns true of the last added handler is executed.

We will see an example when we add another exception handler that will execute before the GlobalExceptionHandler.

Program.cs:

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
