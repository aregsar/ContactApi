# ContactUseExceptions

## Returning Problem Details Error response for unhandled exceptions

When running minimal apis we want to return a consistent  error response when our application throws an exception that is not handled.

We can leverage the ProblemDetails RFC to return a  ProblemDetails json response that all clients can handle in consistent manner.

By default in non development mode the asp.net request pipeline will return a generic error response for any unhandled exceptions.

In development mode the asp.net framework under the hood adds the UseDeveloperExceptionPage middleware to the pipeline that adds additional debugging info like the stack trace.

In this article I will show you how the asp.net UseExceptions middleware and the UseProblemDetails service work together to handle unhandled exceptions and return standard problem details responses.

MOVE THIS:
so that our unhandled exception go through a single unified middleware path in all environments.

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactUseExceptions -o ContactUseExceptions
cd ContactUseExceptions
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
dotnet new web -o ContactUseExceptions
dotnet sln ContactApi.slnx add ContactUseExceptions/ContactUseExceptions.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```
