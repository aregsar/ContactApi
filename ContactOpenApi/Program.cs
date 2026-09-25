using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContactDbContext>(opt => opt.UseInMemoryDatabase("Contacts"));


builder.Services.AddOpenApi(options =>
{
    // Registers the 'BearerAuth' definition globally
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

    // Checks every endpoint and conditionally hooks up the requirement
    options.AddOperationTransformer<BearerOperationTransformer>();
});


// This registers it so IOptions<JwtSettings> can be injected into constructors
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName)
);

// This cleanly reads the section right now for Program.cs without any warnings!
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName)
                                       .Get<JwtSettings>() ?? new JwtSettings();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();
//              .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidIssuer = jwtSettings.Issuer,

//         ValidateAudience = true,
//         ValidAudience = jwtSettings.Audience,

//         ValidateIssuerSigningKey = true,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.JwtSigningKey)),

//         ValidateLifetime = true,
//         ClockSkew = TimeSpan.Zero
//     };
// });


builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Pass your custom prefix here as a direct string parameter!
    //app.MapScalarApiReference("/my-custom-docs", options =>
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Contacts API")
                .AddPreferredSecuritySchemes("BearerAuth");

        options.AddHttpAuthentication("BearerAuth", auth =>
        {
            auth.Token = builder.Configuration["Authentication:JwtToken"];
            //auth.Token = jwtSettings.JwtToken;
        });
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");
ContactsEndpointMapper.Map(app);

app.Run();


public sealed class JwtSettings
{
    public const string SectionName = "Authentication";

    public string JwtSigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
}