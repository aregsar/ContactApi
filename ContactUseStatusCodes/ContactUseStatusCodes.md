# ContactUseStatusCodes

> Uses .NET 10

### Setting up the project skeleton quick start

Prerequisite:

Globally install or update the xunit V3 project templates:

```bash
dotnet new install xunit.v3.templates
```

Create the project solution:

```bash
dotnet new sln -n ContactUseStatusCodes -o ContactUseStatusCodes
cd ContactUseStatusCodes
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Add skeleton:

```bash
dotnet new web -o ContactUseStatusCodes
dotnet sln ContactApi.slnx add ContactUseStatusCodes/ContactUseStatusCodes.csproj
#echo "# ContactUseStatusCodes" >> ContactUseStatusCodes/ContactUseStatusCodes.md
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactUseStatusCodes/ContactUseStatusCodes.csproj
dotnet package add Scalar.AspNetCore --project ContactUseStatusCodes/ContactUseStatusCodes.csproj
touch ContactUseStatusCodes/ContactUseStatusCodes.http
dotnet new xunit3 -f net10.0 -o tests/ContactUseStatusCodes.Tests
dotnet sln ContactApi.slnx add tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj
dotnet add tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj reference ContactUseStatusCodes/ContactUseStatusCodes.csproj
touch ContactUseStatusCodes/ContactUseStatusCodes.http
#dotnet new .gitignore
#echo "# ContactUseStatusCodes" >> README.md
```

## Configure Problem Details for bodyless Error Status Code responses

Adding Problem Details to the http response body for http error status codes responses that have no body.

We will add app.UseStatusCodePages() middleware for handling HTTP error responses in the range of 400–599 that in addition do not have a response body.

The app.UseStatusCodePages() method will add a problemdetails response body using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactUseStatusCodes -o ContactUseStatusCodes
cd ContactUseStatusCodes
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
dotnet new web -o ContactUseStatusCodes
dotnet sln ContactApi.slnx add ContactUseStatusCodes/ContactUseStatusCodes.csproj
#echo "# ContactUseStatusCodes" >> ContactUseStatusCodes/ContactUseStatusCodes.md
```

### Add Open Api package and the Scalar Open Api UI Package

```bash
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactUseStatusCodes/ContactUseStatusCodes.csproj
dotnet package add Scalar.AspNetCore --project ContactUseStatusCodes/ContactUseStatusCodes.csproj
```

### Update Minimal API Code

Add the OpenApi and Scalar UI integration to the generated minimal api boilerplate in Program.cs:

```cs
//using Microsoft.AspNetCore.Http.HttpResults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}



app.MapGet("/", () => "Hello World!");

//this returns text/json content
//app.MapGet("/", Ok<string> () => TypedResults.Ok("Hello World!"));
//this returns text/json content type valid json serialized object MessageResponse
//app.MapGet("/", Ok<MessageResponse> () => TypedResults.Ok(new MessageResponse("Hello World!")));

app.Run();
//public record MessageResponse(string Message);
```

### Adding a .http file for making API requests

Before you add the .http file make sure your editor is configured to support .http files.

> Some editors like Visual Studio or Rider have built in support. VSCode editor needs the `REST Client for Visual Studio Code` extension.

Add the .http file to the project root:

```bash
touch ContactUseStatusCodes/ContactUseStatusCodes.http
```

Add a HTTP GET request to the .http file:

```http
@baseUrl = http://localhost:5095

### Get Root URL
GET {{baseUrl}}/
Accept: application/json
```

Replace the `@baseUrl` port number  with the randomly generated port number of the `applicationUrl` setting in the `http` profile of your projects `Properties/launchSettings.json` file.

### Running the api and sending requests with .http file

Run the the project with the launchSettings.json http profile:

```bash
dotnet run --project ContactUseStatusCodes/ContactUseStatusCodes.csproj --launch-profile http
```

> Note if you omit the --launch-profile flag then the first profile in launchSettings.json file will be used as the default launch profile.

Click on the send request button above the comment line in the .http file to send the GET request to the root URL.

The response should look like:

```bash
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Wed, 29 Jul 2026 20:19:59 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello World!
```

### Sending requests using the scalar web page

Go to Scalar UI in your browser and send the request from the Scalar dashboard page:

```bash
open http://localhost:5095/Scalar/v1
```

### Sending requests using Curl

If you have curl utility installed you can also send a request using the curl cli.

Open another terminal tab and run the curl command to see the same response:

```bash
curl -i -H "Connection: close" http://localhost:5095
```

Remember to change the port number according to your projects launch profile setting.

>Note that with Curl we need to explicitly send the Connection: close header for the Kestrel server to close the connection using. This is something that the VSCode REST Client extension takes care of automatically when we use the .http file to make the request. The HTTP Client extension automatically injects the Connection: close header into the request which is why we get the Connection: close header back in the response when using the .http file to send the request.

### Add Endpoint Tests with XUnit

Install the xunit v3 templates:

> The command need to run only when you need to install or update the templates.

```bash
dotnet new install xunit.v3.templates
```

After installing the templates you can create a test project using the template and add it to the solution:

```bash
dotnet new xunit3 -f net10.0 -o tests/ContactUseStatusCodes.Tests
dotnet sln ContactApi.slnx add tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj
```

Reference the ContactUseStatusCodes.csproj project from the ContactUseStatusCodes.Tests.csproj:

```bash
dotnet add tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj reference ContactUseStatusCodes/ContactUseStatusCodes.csproj
```

The command adds a reference to the ContactUseStatusCodes.Tests.csproj file.

Add a test file to the test project:

```bash
touch ContactStarter.Tests/ContactUseStatusCodesTests.cs
```

Add the root endpoint test to ContactUseStatusCodesTests.cs file:

> The test code is for demo testing only. For production tests we would not create a new WebApplicationFactory for each test method. We would use an IClassFixture instead.

```cs
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactUseStatusCodes.Tests;

public class ContactUseStatusCodesTests
{
    [Fact]
    public async Task GET_RootEndpoint_Returns_200_OK_And_HelloWorld()
    {
        var factory = new WebApplicationFactory<Program>();

        var client = factory.CreateClient();

        // var prodClient = factory.WithWebHostBuilder(builder =>
        //     {
        //         builder.UseEnvironment("Production");
        //     }).CreateClient();

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

        var message = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Hello World!", message);


        //for app.MapGet("/", Ok<string> () => TypedResults.Ok("Hello World!")):
        //   Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        //var message = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        //json response wraps hello world response in quotes
        //   Assert.Equal("\"Hello World!\"", message);

        //for app.MapGet("/", Ok<MessageResponse> () => TypedResults.Ok(new MessageResponse("Hello World!")));
        //   Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        //   MessageResponse? messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>(TestContext.Current.CancellationToken);
        // Assert.NotNull(messageResponse);
        // Assert.Equal("Hello World!", messageResponse?.Message);
    }
}
```

Run the tests:

```bash
# run all tests  in the test project
dotnet run --project tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj

# run all tests in a ContactStarterTests class in the test project
dotnet run --project tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj -- -class ContactUseStatusCodes.Tests.ContactUseStatusCodesTests

# run the GET_RootEndpoint_Returns_200_OK_And_HelloWorld class method test in the project
dotnet run --project tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj -- -method ContactUseStatusCodes.Tests.ContactUseStatusCodesTests.GET_RootEndpoint_Returns_200_OK_And_HelloWorld
```
