# ContactUseStatusCodes

> Uses .NET 10

## Adding Problem Details content to contentless Error Status Code responses

When running minimal apis we want to return a consistent error response in all conditions where the response return an error status code in the  400–599 range.

We can leverage the ProblemDetails RFC to return a json ProblemDetails response that all clients can handle in consistent manner.

By default for certain requests the asp.net request pipeline may return error status code responses without content in the body.

For these cases we want to be able to add a ProblemDetails content to the output stream so that all error responses have problem details content.

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
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

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
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStatusCodePages();

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
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages();

app.MapGet("/", () => "Hello World!");

app.Run();
```

The AddProblemDetails call adds a IProblemDetailsWriter implementation to the service container.

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

The UseStatusCodePages middleware calls the WriteAsyncJson method of the IProblemDetailsWriter passing it a ProblemDetailsContext object that WriteAsyncJson serializes to the output stream.

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

public class ContactUseStatusCodesTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{

    // W3C traceparent header
    // Format: Version-TraceId-SpanId-Flags
    private const string MockTraceParentValue = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    private const string TraceParentRequestHeaderName = "traceparent";

    [Fact]
    public async Task GET_Does_Not_Exist_Endpoint_Returns_404_Not_Found_Status_And_ProblemDetails_Content()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/does/not/exist", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);

        Assert.Equal(404, problem?.Status);

        Assert.Equal("Not Found", problem?.Title);

        Assert.Null(problem?.Detail);

        Assert.Null(problem?.Instance);

    }

    [Fact]
    public async Task GET_Does_Not_Exist_Endpoint_Returns_404_Not_Found_Status_And_ProblemDetails_Content_Injected_Traceparent()
    {

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

        Assert.Null(problem?.Instance);

        //Test that the traceparent header value is used as the traceId
        object? traceId = null;
        Assert.True(problem?.Extensions.TryGetValue("traceId", out traceId));

        Assert.True(ActivityContext.TryParse(traceId?.ToString(), null, out ActivityContext actualContext));

        Assert.True(ActivityContext.TryParse(MockTraceParentValue, null, out ActivityContext expectedContext));

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

### Bonus: Writing custom body response using a delegate

We can pass in a delegate with custom code to write response content to the UseStatusCodePages when we call it.

In this case we are overriding the default middleware code that writes content to the response  stream using IProblemDetailWriter with our own custom code that can also use the IProblemDetailWriter service.

Lets modify Program.cs to pass in the delegate lambda to UseStatusCodePages:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages(async context =>
{
    var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Status = context.HttpContext.Response.StatusCode,
        Title = "Not Found",
    };

    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
        HttpContext = context.HttpContext,
        ProblemDetails = problemDetails
    });
});

app.MapGet("/", () => "Hello World!");

app.Run();
```

The problemDetailsService.WriteAsync method will add the trace id to the ProblemDetails object extensions dictionary, if we don't set it in our custom implementation.

The code inside the delegate lambda is exactly the same as what the default UseStatusCode pages implements so we our test should still pass.

```bash
dotnet run --project tests/ContactUseStatusCodes.Tests/ContactUseStatusCodes.Tests.csproj
```

> Make sure to call builder.Services.AddProblemDetails() so your context.HttpContext.RequestServices.GetRequiredService call does not return null. Otherwise you can add a null check to avoid a null reference exception and fall back to using a standard json writer if that strategy makes sense in your own implementation.

Here is an example of a custom delegate that also sets the problemDetails.Instance and problemDetails.Detail properties and also sets the traceId to  problemDetails.Extensions dictionary instead of letting  problemDetailsService.WriteAsync set it.

```cs
app.UseStatusCodePages(async context =>
{
    var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        Status = context.HttpContext.Response.StatusCode,
        Title = "Not Found",
        Instance = context.HttpContext.Request.Path,
        Detail= "Resource Was Not Found",
    };

    var traceId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    problemDetails.Extensions["traceId"] = traceId;

    await problemDetailsService.WriteAsync(new ProblemDetailsContext
    {
        HttpContext = context.HttpContext,
        ProblemDetails = problemDetails
    });
});

```

If we were to change to this implementation of the delegate, we would also need to change our Datails and Instance property assertions to Assert.NotNull(problem?.Detail) and Assert.NotNull(problem?.Instance) in our ContactUseStatusCodesTests test methods for the tests to pass..

### Extracting the UseStatusCode pages delegate logic to a static method

```bash
touch ContactUseStatusCodes/StatusCodePagesHandler.cs
```

Copy the code inside the delegate into the StatusCodePagesHandlers file:

```cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public static class StatusCodePagesHandler
{
    public static async Task WriteProblemDetailsAsync(StatusCodeContext context)
    {
        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        var problemDetails = new ProblemDetails
        {
            Status = context.HttpContext.Response.StatusCode,
            Title = "Not Found",
            //Instance = context.HttpContext.Request.Path
        };

        // var traceId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        // problemDetails.Extensions["traceId"] = traceId;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context.HttpContext,
            ProblemDetails = problemDetails
        });
    }
}
```

Now we can call our custom status code pages handler with a single line:

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages(StatusCodePagesHandler.WriteProblemDetailsAsync);

app.MapGet("/", () => "Hello World!");

app.Run();
```

> If you have no need to customize the status code pages specific problem details output then calling the default argument less UseStatusCodePages is recommended.

There is also another way to customize problem details globally regardless of the source of the error that we will cover in another article.

## Conclusion

We saw how we can use the UseStatusCodePages middleware along with the IProblemDetailsService that is added by AddProblemDetails service builder to write ProblemDetails json response in http error status code responses that would otherwise have empty body content.

This enables us to be consistent with the error responses that we send back to our api clients for any type of error.

We also saw that we can also write the request trace identifier in the content, which will aid us in tracing and debugging our service requests.

Along the way we got to see how the asp.net middlewar pipeline works under the hood to handle and write the error responses to the outout stream.

Finally we wrote tests to validate that our pipeline is returning the proper Problem Details response for 404 not found status errors.

In furure related articles we will see how global exception handling and global problem details handling works alongside UseStatusPages to handle application errors and problemdetails formatting in a uniform and comprehensive way for all our Minimal API projects.
