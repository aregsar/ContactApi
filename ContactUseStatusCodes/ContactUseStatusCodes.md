# ContactUseStatusCodes

> Uses .NET 10

## Adding Problem Details content to contentless Error Status Code responses

When running minimal apis we want to return a consistant error response in all conditions where the response return an error status code in the  400–599 range.

We can leverage the ProblemDetails RFC to return a json ProblemDetails response that all clients han handle in consistant manner.

By default for certains requests the asp.net request pipeline may return error status code responses without content in the body.

For these cases we want to be able to add a ProblemDetails content to the output stream to remain consistant.

In this article I will show you how the asp.net UseStatusCodePages middleware and the UseProblemDetails service work together to accomplish this.

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

app.Run();
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
GET {{baseUrl}}/does/not/exist
Accept: application/json
```

Replace the `@baseUrl` port number  with the randomly generated port number of the `applicationUrl` setting in the `http` profile of your projects `Properties/launchSettings.json` file.

### Running the api and sending requests with .http file

Run the the project with the launchSettings.json http profile:

```bash
dotnet watch --project ContactUseStatusCodes/ContactUseStatusCodes.csproj --launch-profile http
```

> Note if you omit the --launch-profile flag then the first profile in launchSettings.json file will be used as the default launch profile.

Click on the `send request` button right below the comment line in the .http file to send the GET request to the `/does/not/exist` URL.

The response should look like:

```bash
HTTP/1.1 404 Not Found
Content-Length: 0
Connection: close
Date: Mon, 05 Oct 2026 18:05:35 GMT
Server: Kestrel
```

As we can see we get a 404 Not Found status code since we have not mapped the `/does/not/exist` URL.

We also see that the response does not have a body content.

We need to add the UseStatusCodePages middleware to intercept the empty response and add a ProblemDetails json content body to it.

The UseStatusCodePages will only intercept and alter the response only if all the following is true:

- The HTTP status code is between 400 and 599.
- The response body length is zero, which means no content is written to the response stream.
- The No Content-Type: The response Content-Type header has not been

So lets add the middleware to the Program.cs file:

```cs
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");

app.Run();
```

You can see we added the `app.UseStatusCodePages();` middleware line.

> the dotnet watch hot reload does not reload the middleware pipeline unless you are only changing a delegate passed to it. So we need to reload it manually when change the pipeline.

To reload the updated middleware pipeline, Type `ctrl + R` in the terminal window where dotnet watch is running to rerun it.

Click on the send request button in the .http file to send the the request again.

The response should look like:

```http
HTTP/1.1 404 Not Found
Connection: close
Content-Type: text/plain
Date: Mon, 05 Oct 2026 18:26:03 GMT
Server: Kestrel
Transfer-Encoding: chunked

Status Code: 404; Not Found
```

Here we see the generic body content that the middleware adds: `Status Code: 404; Not Found`

We have not enabled it to write problem details json yet.

To do that we need call builder.Services.AddProblemDetails().

Lets add the call to Program.cs:

```cs
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World! ok");

app.Run();
```

The AddProblemDetails call registers a IProblemDetailsWriter implementation with the service container.

If the UseStatusCodePages middleware finds that service in the container, it will use it to write a ProblemDetails response json content to the response stream.

If UseStatusCodePages cant find it, falls back on the generic status code content output that we saw earlier.

Now that we added the service we should see the ProblemDetails output once we reload the middleware pipeline by typing `ctrl + R` again.

Now click on the send request button in the .http file to send the the request again.

The response should look like:

```http
HTTP/1.1 404 Not Found
Connection: close
Content-Type: application/problem+json
Date: Mon, 05 Oct 2026 18:27:15 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-cca8ee2c4772fa89cc7e1ef11b068ef1-c2b5ea56193c42f2-00"
}
```

As we can see now we have a json response in the body with the status and a traceId for the request.

The UseStatusCodePages middleware calls the WriteAsyncJson merhod of the IProblemDetailsWriter passing it a ProblemDetailsContext object that WriteAsyncJson serializes to the output stream.

### Viewing the request in a Web browser

You can navigate to <<http://localhost>:<PORT>/does/not/exist> in your browser to check the response with the ProblemDetail service and middleware installed.

```bash
open http://localhost:5095/does/not/exist
```

You should see the same ProblemDetails json content displayed in the page:

```http
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-cca8ee2c4772fa89cc7e1ef11b068ef1-c2b5ea56193c42f2-00"
}
```

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

Add the Microsoft.AspNetCore.Mvc.Testing package to the test project:

```bash
dotnet package add Microsoft.AspNetCore.Mvc.Testing --project tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj
```

Add a test file to the test project:

```bash
touch tests/ContactUseStatusCodes.Tests/ContactUseStatusCodesTests.cs
```

Add the root endpoint test to ContactUseStatusCodesTests.cs file:

> The test code is for demo testing only. For production tests we would not create a new WebApplicationFactory for each test method. We would use an IClassFixture instead.

```cs
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ContactUseStatusCodes.Tests;

public class ContactUseStatusCodesTests
{

    // W3C traceparent header
    // Format: Version-TraceId-SpanId-Flags
    private const string MockTraceParentValue = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    private const string TraceParentRequestHeaderName = "traceparent";

    [Fact]
    public async Task GET_Does_Not_Exist_Endpoint_Returns_404_Not_Found_Status_And_ProblemDetails_Content()
    {
        var factory = new WebApplicationFactory<Program>();

        var client = factory.CreateClient();

        var response = await client.GetAsync("/does/not/exist", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);

        Assert.Equal(404, problem?.Status);

        Assert.Equal("Not Found", problem?.Title);

        Assert.Null(problem?.Detail);

    }

    [Fact]
    public async Task GET_Does_Not_Exist_Endpoint_Returns_404_Not_Found_Status_And_ProblemDetails_Content_Injected_Traceparent()
    {
        ///does/not/exist
        var factory = new WebApplicationFactory<Program>();

        var client = factory.CreateClient();

        //these three lines are same as await client.GetAsync("/does/not/exist", TestContext.Current.CancellationToken) call
        //but with the added header
        var request = new HttpRequestMessage(HttpMethod.Get, "/does/not/exist");
        request.Headers.Add(TraceParentRequestHeaderName, MockTraceParentValue);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);

        Assert.Equal(404, problem?.Status);

        Assert.Equal("Not Found", problem?.Title);

        Assert.Null(problem?.Detail);

        //Test traceparent
        object? traceId = null;
        Assert.True(problem?.Extensions.TryGetValue("traceId", out traceId));
        var actualContext = ActivityContext.Parse(traceId?.ToString(), null);
        var expectedContext = ActivityContext.Parse(MockTraceParentValue, null);
        Assert.Equal(expectedContext.TraceId, actualContext.TraceId);

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

### Bonus: Writing custom body reponse using a delegate

We can pass in a delegate with custom code to write response content to the UseStatusCodePages when we call it.

In this case we are overriding the default middleware code that writes content to the response  stream using IProblemDetailWriter with our own custom code that can also use the IProblemDetailWriter service.
