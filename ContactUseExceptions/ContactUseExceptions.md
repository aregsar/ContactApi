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

In console log we see :

```bash
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.Exception: error
         at Program.<>c.<<Main>$>b__0_1() in /Users/aregsarkissian/RiderProjects/ContactApi/ContactUseExceptions/Program.cs:line 9
         at lambda_method2(Closure, Object, HttpContext)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

```

We can see the Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke call.

By default in development mode the asp.net framework under the hood adds the UseDeveloperExceptionPage middleware to the pipeline that writes debugging info like the stack trace.

However the output is in plain text and not in problem details json format.

We will fix that shortly.

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

## Adding the UseExceptionHandler middleware

lets add the UseExceptionHandler middleware to Program.cs

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

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

Make the same request again an we will the different output:

```http
HTTP/1.1 500 Internal Server Error
Content-Length: 0
Connection: close
Date: Mon, 17 Aug 2026 20:01:05 GMT
Server: Kestrel
```

An exception is thrown
