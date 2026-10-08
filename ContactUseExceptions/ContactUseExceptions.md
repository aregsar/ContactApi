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
GET {{baseUrl}}/does/not/exist
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

Then we can change the value of the `ASPNETCORE_ENVIRONMENT` property of this new `http.prod` profile to `"Production"`.

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
