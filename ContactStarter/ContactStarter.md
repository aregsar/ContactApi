# ContactStarter

## Starter Template for all blog projects

Starter minimal api project.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactStarter -o ContactStarter
cd ContactStarter
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactStarter" >> README.md
```

### Create the project boilerplate

Create a basic project boilerplate:

> All following commands will be run from the solution root directory

```bash
dotnet new web -o ContactStarter
dotnet sln ContactApi.slnx add ContactStarter/ContactStarter.csproj
```

### Add Open Api package and the Scalar Open Api UI Package

```bash
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactStarter/ContactStarter.csproj
dotnet package add Scalar.AspNetCore --project ContactStarter/ContactStarter.csproj
```

### Boilerplate Code

Add a basic minimal api boilerplate that includes the OpenApi and Scalar UI integration:

```cs
//using Microsoft.AspNetCore.Http.HttpResults;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
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

### Adding a http request to the ContactStarter.http file

Before you add the .http file make sure your editor is configured to support .http files.

> Some editors like Visual Studio or Rider have built in support. VSCode editor needs the `REST Client for Visual Studio Code` extension.

```bash
touch ContactStarter/ContactStarter.http
```

Add a HTTP GET request:

```http
@baseUrl = http://localhost:5095

### Get Root URL
GET {{baseUrl}}/
Accept: application/json
```

The `Properties/launchSettings.json` file in the project contains a `http` profile.

Use the  `applicationUrl` setting randomly generated port number in the `http` profile of your project as the `@baseUrl` port.

### Running the api and sending requests

Run the the project with the launchSettings.json http profile:

```bash
dotnet run --project ContactStarter/ContactStarter.csproj --launch-profile http
```

> Note if you omit the --launch-profile flag then the first profile in launchSettings will be used as the default profile.

Click on the send request button above the Get Root URL request in the ContactStarter.http file to send a request and view the response:

```bash
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Wed, 29 Jul 2026 20:19:59 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello World!
```

Go to Scalar UI in your browser and send the request from the Scalar dashboard page:

```bash
open http://localhost:5095/Scalar/v1
```

If you have curl utility installed you can also send a request using curl as well.

Open another terminal tab and run the curl command to see the same response:

```bash
curl -i -H "Connection: close" http://localhost:5095
```

>Note that with Curl we need to explicitly send the Connection: close header for the Kestrel server to close the connection using. This is something that the VSCode REST Client extension takes care of automatically when we use the .http file to make the request. The HTTP Client extension automatically injects the Connection: close header into the request which is why we get the Connection: close header back in the response when using the .http file to send the request.

### Add Endpoint Tests with XUnit

Make sure you have the xunit v3 templates installed (need to run one time only)

```bash
dotnet new install xunit.v3.templates
```

After installing the templates you can create a test project using the template and add it to the solution:

```bash
dotnet new xunit3 -f net10.0 -o tests/ContactStarter.Tests
dotnet sln ContactApi.slnx add tests/ContactStarter.Tests/ContactStarter.Tests.csproj
```

Add a reference to the ContactStarter.Tests.csproj file that references the ContactStarter.csproj project:

```bash
dotnet add tests/ContactStarter.Tests/ContactStarter.Tests.csproj reference ContactStarter/ContactStarter.csproj
```

Add a test file to the test project:

```bash
touch ContactStarter.Tests/ContactStarterTests.cs
```

Add the root endpoint test to ContactStarterTests.cs file:

> The test code is for demo testing only. For production tests we would not create a new WebApplicationFactory for each test method and use an IClassFixture instead.

```cs
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactStarter.Tests;

public class ContactStarterTests
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



        //   Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        //    MessageResponse? messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>(TestContext.Current.CancellationToken);

        // Assert.NotNull(messageResponse);
        // Assert.Equal("Hello World!", messageResponse?.Message);
    }
}
```

Run the tests:

```bash
# run all tests  in the test project
dotnet run --project tests/ContactStarter.Tests/ContactStarter.Tests.csproj

# run all tests in a ContactStarterTests class in the test project
dotnet run --project tests/ContactStarter.Tests/ContactStarter.Tests.csproj -- -class ContactStarter.Tests.ContactStarterTests

# run the GET_RootEndpoint_Returns_200_OK_And_HelloWorld class method test in the project
dotnet run --project tests/ContactStarter.Tests/ContactStarter.Tests.csproj -- -method ContactStarter.Tests.ContactStarterTests.GET_RootEndpoint_Returns_200_OK_And_HelloWorld
```
