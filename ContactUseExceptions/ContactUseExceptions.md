# ContactUseExceptions

## Configure Problem Details Error response for unhandled exceptions

Writing Problem Details http response for unhandled execeptions.

We will add app.UseExceptions() middleware for handling unhandled exceptions.

The app.UseExceptions() method will write a problemdetails error response using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
dotnet new web -o ContactUseExceptions
dotnet sln ContactApi.slnx add ContactUseExceptions/ContactUseExceptions.csproj
echo "# ContactUseExceptions" >> ContactUseExceptions/ContactUseExceptions.md
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash


```
