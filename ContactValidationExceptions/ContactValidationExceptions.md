# ContactValidationExceptions

## Configure Problem Details Error response for unhandled validation exceptions

Writing Problem Details http response for unhandled validation execeptions.

The Validation Exception handler will write a problemdetails error response using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
dotnet new web -o ContactValidationExceptions
dotnet sln ContactApi.slnx add ContactValidationExceptions/ContactValidationExceptions.csproj
echo "# ContactValidationExceptions" >> ContactValidationExceptions/ContactValidationExceptions.md
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash


```
