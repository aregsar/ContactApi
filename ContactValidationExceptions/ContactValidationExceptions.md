# ContactValidationExceptions

## Configure Problem Details Error response for unhandled validation exceptions

Writing Problem Details http response for unhandled validation execeptions.

The Validation Exception handler will write a problemdetails error response using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

### Creating the project solution

Create a solution to host the project and enter the solution directory that is created:

```bash
dotnet new sln -n ContactValidationExceptions -o ContactValidationExceptions
cd ContactValidationExceptions
```

> All following dotnet cli commands will be executed from the root directory of the solution.

Optionally add a .gitignore and README.md file to the solution:

```bash
dotnet new .gitignore
echo "# ContactValidationExceptions" >> README.md
```

### Create the project and add to solution

Create a basic project boilerplate and add it to the solution (.slnx) file:

```bash
dotnet new web -o ContactValidationExceptions
dotnet sln ContactApi.slnx add ContactValidationExceptions/ContactValidationExceptions.csproj
```

The project creates a Program.cs file that contains the following Minimal API application code:

Program.cs:

```cs
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```
