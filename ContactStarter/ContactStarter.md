# ContactStarter

## Starter Template for all blog projects

Starter minimal api project.

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
dotnet new web -o ContactStarter
dotnet sln ContactApi.slnx add ContactStarter/ContactStarter.csproj
echo "# ContactStarter" >> ContactStarter/ContactStarter.md
echo "### ContactStarter Http Requests" >> ContactStarter/ContactStarter.http
```

> All following commands will be run from the solution root directory

### Add Open Api package and the Scalar Open Api UI Package

```bash
dotnet package add Microsoft.AspNetCore.OpenApi --project ContactStarter/ContactStarter.csproj
dotnet package add Scalar.AspNetCore --project ContactStarter/ContactStarter.csproj
```

### Boilerplate Code

Add a basic minimal api boilerplate that includes the OpenApi and Scalar UI integration:

```cs
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

### Running the api and sending requests

### Testing

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
    }
}
```
