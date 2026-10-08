# ContactUseExceptions

## Configure Problem Details Error response for unhandled exceptions

Writing Problem Details http response for unhandled exceptions.

We will add app.UseExceptions() middleware for handling unhandled exceptions.

The app.UseExceptions() method will write a problem details error response using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

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
