# ContactUseStatusCodes

## Configure Problem Details for bodyless Error Status Code responses

Adding Problem Details to the http response body for http error status codes responses that have no body.

We will add app.UseStatusCodePages() middleware for handling HTTP error responses in the range of 400–599 that in addition do not have a response body.

The app.UseStatusCodePages() method will add a problemdetails response body using the IProblemDetailsFormatter implementation registered with the service container by the app.UseProblemDetails() method

### Create the project boilerplate

Use the project boilerplate from Creating a minimal Minimal API project boilerplate post

```bash
dotnet new web -o ContactUseStatusCodes
dotnet sln ContactApi.slnx add ContactUseStatusCodes/ContactUseStatusCodes.csproj
echo "# ContactUseStatusCodes" >> ContactUseStatusCodes/ContactUseStatusCodes.md
```

> All following commands will be run from the solution root directory

### Add Required Packages

```bash


```
