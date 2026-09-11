## Creating a minimal Minimal API project boilerplate

In this post we will create the most basic Minimal API project that has a single endpoint mapped to the root URL of the API.

In future posts we will use this minimal boilerplate to cover individual features of the Minimal API framework and the ASP.NET request pipeline.

To understand how a Minimal API project works, we will inspect the project files and how to configure application settings and run the project.

We will then run the project and inspect the response to http requests.

The post uses the dotnet cli and assumes you have .NET 10 framework installed.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactApi -o ContactApi
cd ContactApi
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new gitignore
echo "# ContactApi" >> README.md
```

### Creating and adding the project to the solution

Create the Minimal API project:

```bash
dotnet new web -o ContactApi
```

Add the project to the solution:

```bash
dotnet sln ContactApi.slnx add ContactApi/ContactApi.csproj
```

### Exploring the solution and project files and directory structure

Display the solution directory structure:

```bash
tree -L 3
```

Here is what the directory structure will look like (excluding the obj and bin directories):

```bash
├── ContactApi.slnx
├── README.md
├── .gitignore
└── ContactApi
       ├── appsettings.Development.json
       ├── appsettings.json
       ├── ContactApi.csproj
       ├── Program.cs
       ├── Properties
            └── launchSettings.json
```

Inspect the ContactApi.slnx solution file:

```bash
cat ContactApi.slnx
```

ContactApi.slnx:

```xml
<Solution>
  <Project Path="ContactApi/ContactApi.csproj" />
</Solution>
```

Inspect the project file ContactApi.csproj:

```bash
cat ContactApi/ContactApi.csproj
```

ContactApi.csproj:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

### Exploring the Minimal API application settings

Explore the appsettings.json configuration file:

```bash
cat ContactApi/appsettings.json
```

appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

The default `Logging:LogLevel` setting sets the default LogLevel for all logger logging calls to `Information`.

The `Microsoft.AspNetCore` setting overrides this default level to `Warning` for logging calls in classes from the `Microsoft.AspNetCore` namespace.

We can add additional overrides of our own for other namespaces.

Namespaces down a level in the namespace hierarchy will override the parent namespace setting.

So for example if we had the settings `"Microsoft": "Information"` and `"Microsoft.AspNetCore": "Warning"`, then the log level of logs from `Microsoft` namespace would be `Information` while the log level of logs from `Microsoft.AspNetCore` namespace would be `Warning`.

Explore appsettings.development.json:

```bash
cat ContactApi/appsettings.development.json
```

appsettings.development.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

When the application runs in a development environment, the settings in appsettings.development.json will override settings in the default appsettings.json file.

In this case since the LogLevel settings are the same in both files, the override does not change any values.

### Looking at the Minimal API Program.cs entry point

Explore the minimal api code in the Program.cs file:

```bash
cat ContactApi/Program.cs
```

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

This is the bare minimum code you need to run a Minimal Api project.

The code creates a web application builder and does not configure any services using the builder.

Then is calls `builder.Build()` to create the web application.

Then the application maps a single endpoint that returns `Hello World` to the root `/` URL.

Finally the call to `app.Run()` runs the application.

### Exploring the application launch settings

Explore the properties/launchSettings.json file:

```bash
cat ContactApi/Properties/launchSettings.json
```

ContactApi/Properties/launchSettings.json:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5014",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7095;http://localhost:5014",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

There are two profiles in this file.

When the application is run, the run command selects one of the profiles in
the `properties/launchSettings.json` file to configure host environment settings and run the application.

The applicationUrl setting of the selected profile is used to run the Kestrel application server with the host, port and scheme of the setting.

The environment that the application runs in is set to the ASPNETCORE_ENVIRONMENT setting value of the selected profile.

The first profile in the `properties/launchSettings.json` file is the http profile.

When we run the project without specifying the profile, the first profile is selected by default.

Run the project using the default (http) profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj
```

We can be explicit and specify the profile we want to run by adding it as a flag when we run the project.

Run the project by explicitly specifying the http profile as the launch profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile http
```

Both previous run commands run the application using the http profile.

Now run the project by specifying the https profile as the launch profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile https
```

> Note that the `https` profile actually runs the Kestrel server using both the http and https schemes as specified in the applicationUrl setting.

The first time you run the https profile, you will be prompted to install a self signed certificate.

To install the certificate you need to run:

```bash
dotnet dev-certs https --trust
```

### Adding a Production profile to launchsettings

Lets add a third launch profile to launchsettings.json that sets the environment to Production.

```json
{
    "$schema": "https://json.schemastore.org/launchsettings.json",
    "profiles": {
        "http": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "http://localhost:5014",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        "http.prod": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "http://localhost:5014",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Production"
            }
        },
        "https": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "applicationUrl": "https://localhost:7095;http://localhost:5014",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
    }
}
```

This new profile is named http.prod and we set its ASPNETCORE_ENVIRONMENT setting to Production.

We can use this new profile to launch the project to test application behavior in a Production environment.

Launch the project using the Production profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile http.prod
```

In the terminal we can see we are running this production profile:

```bash
Using launch settings from ContactApi/Properties/launchSettings.json...
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5014
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/aregsarkissian/RiderProjects/ContactApi/ContactApi
```

### Sending requests to the api using .http files

One way we can send requests to our running Miniaml API application is to use .http files.

These .http files are files with the .http extension that contain http request scripts that can send requests to specified endpoints.

VSCode has support for them using the VSCode HTTP Client extension.

All major IDEs and Editors have either native support for them or support them through plugins.

Using this support you can easily send requests by clicking on links in the document.

As a bonus these .http documents can be checked into source control.

Add a `ContactApi.http` to the `ContactApi` project root.

In the root directory of the solution type:

```bash
touch ContactApi/ContactApi.http
```

Add the following content to the .http file:

```http
@baseUrl = http://localhost:5014

### Get Root URL
GET {{baseUrl}}/
Accept: application/json

### Get Non Existant URL
GET {{baseUrl}}/doesnotexist
Accept: application/json
```

This file has two http get requests specified.

The first is a request to the root `/` URL that is mapped in Program.cs.

The second is a request to a dummy `/doesnotexist` URL that is not mapped in our project.

A `@baseUrl` variable is defined that is used in both requests.

The lines that start with `###` are comment lines and are used as separators between requests.

The `@baseUrl` variable is set the to the common applicationUrl value from the `http` and `http.prod` profiles of the `properties/launchsettings.json` file.

When we run the project with the `http` or `http.prod` profile the server will respond to the requests from the .http file.

Lets try it.

Run the default http profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj
```

Now go to the .http file in your editor and click the send request button next to each request.

The response to the GET `/` request is:

```http
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Thu, 10 Sep 2026 20:56:21 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello World!
```

The response to the GET `/notfound` request is:

```http
HTTP/1.1 404 Not Found
Content-Length: 0
Connection: close
Date: Thu, 10 Sep 2026 20:56:32 GMT
Server: Kestrel
```

### Using CURL to make requests

We can also use the CURL cli to make the same requests to our api.

Open another terminal tab and run the curl command to see the same responses as before:

```bash
curl -i -H "Connection: close" http://localhost:5014/
```

The response will be:

```bash
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Thu, 10 Sep 2026 20:56:56 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello World!
```

```bash
curl -i -H "Connection: close" http://localhost:5014/doesnotexist
```

The response will be:

```bash
HTTP/1.1 404 Not Found
Content-Length: 0
Connection: close
Date: Thu, 10 Sep 2026 20:57:23 GMT
Server: Kestrel
```

> Note that with Curl we need to explicitly send the Connection: close header for the Kestrel server to close the connection using. This is something that the VSCode HTTP Client extension takes care of automatically when we use the .http file to make the request. The HTTP Client extension automatically injects the Connection: close header into the request which is why we get the Connection: close header back in the response.

### Removing the Kestrel server header from Production responses

As a bonus, we can remove the `Server: Kestrel` header from the response returned by ASP.NET in production.

Update the Program.cs file with the following:

Program.cs

```cs
var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
  // Remove the Kestrel Server header when not in development mode
  builder.WebHost.ConfigureKestrel(options =>
  {
    options.AddServerHeader = false;
  });
}

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

We added a code section right after the builder is created that configures the ASP.NET pipeline.

The code configures the pipeline to exclude the Kestrel server header, but only when not running in development.

Run the app in Production environment:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile http.prod
```

Send the request to the root URL and check the response:

```http
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Thu, 10 Sep 2026 20:59:39 GMT
Transfer-Encoding: chunked

Hello World!
```

You should see that the `Server: Kestrel` response header was removed from the response.

Same should apply when you send the request to the `/doesnotexist` URL

Now Run the app in Development environment:

```bash
dotnet run --project ContactApi/ContactApi.csproj
```

Send the same requests and check the responses.

Here is the response to the root URL:

```http
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Thu, 10 Sep 2026 20:56:21 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello World!
```

You should see the Server: Kestrel header reappear in the response.

## Conclusion

We saw how to create a modern .NET solution (.slnx file) and add the bare minimum of Minimal API project to the solution.

Then we explored the files and directories in the project and solution and how they work.

Finally we explored how to launch the project, send http requests to the API and view the response.
