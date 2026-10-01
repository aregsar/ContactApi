# ContactProblemDetails

## Configure Problem Details writer service and global Problem Details serialization interceptor

A problemdetails http response is written using the IProblemDetailsWriter implementation registered with the service container by the app.UseProblemDetails() method

Before IProblemDetailsWriter implementation serializes problemdetails object it can call a global pre-serialization interceptor if an interceptor callback is registered.

The callback can modify the problemdetails object before it is serialized to the output stream.

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
dotnet new web -o ContactProblemDetails
dotnet sln ContactApi.slnx add ContactProblemDetails/ContactProblemDetails.csproj
echo "# ContactProblemDetails" >> ContactProblemDetails/ContactProblemDetails.md
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash


```
