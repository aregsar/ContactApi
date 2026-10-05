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

        object? traceId = null;
        Assert.True(problem?.Extensions.TryGetValue("traceId", out traceId));
        Assert.Equal(MockTraceParentValue, traceId?.ToString());
    }
}
