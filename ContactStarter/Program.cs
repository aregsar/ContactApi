using Microsoft.AspNetCore.Http.HttpResults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");

//this returns text/json content
//app.MapGet("/a", Ok<string> () => TypedResults.Ok("Hello World!"));
//this returns text/json content type valid json serialized object MessageResponse
//app.MapGet("/b", Ok<MessageResponse> () => TypedResults.Ok(new MessageResponse("Hello World!")));

app.Run();
public record MessageResponse(string Message);