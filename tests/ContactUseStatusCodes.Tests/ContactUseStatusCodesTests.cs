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

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

        //var message = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);


    }
}
