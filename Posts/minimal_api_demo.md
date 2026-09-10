## Creating a minimal Minimal API project

In this post we will create a minimal api project template with a single endpoint mapped to the root URL.

We will inspect the files for the project then run the project and inspect the response to our http requests.

In future posts we will use this minimal template to cover individual features of the asp.net and minimal api request pipeline and web applications.

The post uses the dotnet cli and assumes you have .NET 10 framework installed.

Create the solution for the project and go to its directory:

```bash
dotnet new sln -n ContactApi -o ContactApi
cd ContactApi
```

> All following dotnet cli commands will be executed from the root directory of the solution

Optinally add a .gitignore and README.md file:

```bash
dotnet new gitignore
echo "# ContactApi" >> README.md
```

Create the Minimal API project:

```bash
dotnet new web -o ContactApi
```

Explore the solution directory structure:

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

Add the project to the solution:

```bash
dotnet sln ContactApi.slnx add ContactApi/ContactApi.csproj
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

Explore the project file ContactApi.csproj:

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

The default log level setting sets the default LogLevel for all ILogger.log calls to Information.

The Microsoft.AspNetCore setting overrides the LogLevel of the ILogger.log calls for classes in the Microsoft.AspNetCore namespace to Warning.

We can add additional overrides for other namespaces.

Namespaces down a level in the namespace hierarchy will override the parent namespace setting.

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

The settings in appsettings.development.json override settings in appsettings.json when the applcation runs in a develelopment environment.
In this case the LogLevel settings are the same in both files.

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

The code creates a application builder, does not configure any services and creates the application using the builder.

Then the application maps the single root endpoint and then runs the application.

This is the bare minimum code you need to run a Minimal Api project

When the application is run it uses the configuration in the properties/launchSettings.json file to set the server URL and the environment the application is run in.

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

There are two profiles in this file. The first is the http profile and when we run the project without specifying the profile, this first profile is selected by default.

We can be explicit and specify the profile we want to run by adding it as a flag when we run the project.

Run the project using the default (http) profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj
```

Run the project by explicitly specifying the http profile as the launch profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile http
```

Both previous runs launch using the same http profile.

Run the project by specifying the https profile as the launch profile:

Run the project by specifying the https profile as the launch profile:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile https
```

> If you run the https profile, you will be prompted to install a self signed certificate. To install the certificate you need to run: dotnet cert install

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

We can use this new profile to launch the project to test application behaviour in Production environment.

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

### Adding a .http file

In the root directory of the solution type:

```bash
touch ContactApi/ContactApi.http
```

> In VSCode you need to have the VSCode HTTP Client extension installed to be able to send the requests from within the .http file

Add the following content to the file:

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

The first is a request to the root `/` URL that is mapped in Program.cs and returns "Hello World!".

The second is a request to a `/notfound` URL that is not configured in our project.

A `@baseUrl` variable is defined that is used in both requests.

The lines that start with `###` are comment lines and are used as separators between requests.

The `@baseUrl` is set the to the applicationUrl value from the `http` and `http.prod` profiles of our `properties/launchsettings.json` file.

So when we run the project with the `http` or `http.prod` profile the server should respond to the requests in the .http file.

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

We can also use the Curl cli to make the same requests to our api.

Open another terminal tab and run the curl command to see the same responses:

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

### Bonus -  Remove the Kestrel server header from responses in Production mode

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

Run the app in Production environment:

```bash
dotnet run --project ContactApi/ContactApi.csproj --launch-profile http.prod
```

Send the same requests and check the responses.

You shouln't see the Server: Kestrel response header.

```http
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Thu, 10 Sep 2026 20:59:39 GMT
Transfer-Encoding: chunked

Hello World!
```

Run the app in Development environment:

```bash
dotnet run --project ContactApi/ContactApi.csproj
```

Send the same requests and check the responses.

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

We saw how to create a modern .NET solution (.slnx file) and add the bare minimum of Mininal API project to the solution.

Then we explored the files and directories in the project and how they work.

Finally we explores how to launch the project, send http requests to the API and view the response.

We saw that we  get a 404 not found response when an endpoint is not mapped for a requested URL.
