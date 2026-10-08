# ContactProblemDetails

## Configure Problem Details writer service and global Problem Details serialization interceptor

A problemdetails http response is written using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

Before IProblemDetailsWriter implementation serializes problemdetails object it can call a global pre-serialization interceptor if an interceptor callback is registered.

The callback can modify the problemdetails object before it is serialized to the output stream.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactProblemDetails -o ContactProblemDetails
cd ContactProblemDetails
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactProblemDetails" >> README.md
```

### Create the project and add to solution

Create a basic project boilerplate and add it to the solution (.slnx) file:

```bash
dotnet new web -o ContactProblemDetails
dotnet sln ContactApi.slnx add ContactProblemDetails/ContactProblemDetails.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```
