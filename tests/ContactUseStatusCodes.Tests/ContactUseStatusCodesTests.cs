using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;// for   builder.ConfigureLogging
using Microsoft.Extensions.Logging;

namespace ContactUseStatusCodes.Tests;


/*
dotnet test --logger "console;verbosity=detailed"
dotnet test -p:TestingPlatformCaptureOutput=false
To make sure background thread logs originating from WebApplicationFactory can safely map back to xUnit, add this attribute to any .cs file in your test project (e.g., above your test class or in an AssemblyInfo.cs file
using Xunit;
// This forces xUnit v3 to intercept and bind all console outputs
// coming from any thread in the test project
[assembly: CaptureConsole]
With the attribute in place, you can throw away the custom XUnitLoggingProvider and the factory.WithWebHostBuilder code entirely. You don't need them.
Because [assembly: CaptureConsole] routes standard outputs to xUnit, you can use standard Console.WriteLine or standard app logging seamlessly
*/
public class ContactUseStatusCodesTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutput) : IClassFixture<WebApplicationFactory<Program>>
{

    // W3C traceparent header
    // Format: Version-TraceId-SpanId-Flags
    private const string MockTraceParentValue = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
    private const string TraceParentRequestHeaderName = "traceparent";

    [Fact]
    public async Task GET_Does_Not_Exist_Endpoint_Returns_404_Not_Found_Status_And_ProblemDetails_Content()
    {
        //Console.WriteLine("--- Starting Test Sequence ---");
        ////TestContext.Current.SendDiagnosticMessage("Hello Test");

        //var output = TestContext.Current.TestOutputHelper;

        testOutput.WriteLine("--- Starting Test Sequence ---");

        factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                // Clears console/debug logs and sends everything to xUnit
                logging.ClearProviders();

                logging.AddProvider(new XUnitLoggingProvider(testOutput));

            });
        });

        var client = factory.CreateClient();

        testOutput.WriteLine("--- Sending HTTP Request ---");

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
