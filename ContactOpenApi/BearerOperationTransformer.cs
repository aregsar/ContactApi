using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text;


public sealed class BearerOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Document is null)
        {
            return Task.CompletedTask;
        }

        // 1. Check for anonymous access bypass
        var hasAllowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (hasAllowAnonymous)
        {
            return Task.CompletedTask;
        }

        // 3. Document the specific security scheme requirement (the Lock Icon)
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        var schemeReference = new OpenApiSecuritySchemeReference("BearerAuth", context.Document);
        var requirement = new OpenApiSecurityRequirement
        {
            [schemeReference] = []
        };
        operation.Security.Add(requirement);

        // 4. Inject standard response codes for secured endpoints
        operation.Responses ??= new OpenApiResponses();

        // Ensure ProblemDetails schema is registered and retrieve it
        var problemDetailsSchema = GetOrCreateProblemDetailsSchema(context.Document);

        // Ensure 401 Unauthorized is documented
        if (!operation.Responses.ContainsKey("401"))
        {
            var response401 = new OpenApiResponse
            {
                Description = "Unauthorized - Valid JWT token missing or expired.",
                Content = new Dictionary<string, IOpenApiMediaType>(),
            };
            response401.Content["application/problem+json"] = new OpenApiMediaType { Schema = problemDetailsSchema };
            operation.Responses["401"] = response401;
        }

        // 2. Fetch all AuthorizeAttributes
        var authAttributes = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .ToList();

        // 5. Check for roles/policies to conditionally add a 403 Forbidden response
        var roles = authAttributes.Where(a => !string.IsNullOrEmpty(a.Roles)).Select(a => a.Roles).ToList();
        var policies = authAttributes.Where(a => !string.IsNullOrEmpty(a.Policy)).Select(a => a.Policy).ToList();

        if (roles.Count > 0 || policies.Count > 0)
        {
            // Document 403 Forbidden since granular authorization rules exist
            if (!operation.Responses.ContainsKey("403"))
            {

                var response403 = new OpenApiResponse
                {
                    Description = "Forbidden - Authenticated user lacks the required roles or policy permissions.",
                    Content = new Dictionary<string, IOpenApiMediaType>(),
                };
                response403.Content["application/problem+json"] = new OpenApiMediaType { Schema = problemDetailsSchema };
                operation.Responses["403"] = response403;
            }

            // 6. Build a beautiful Markdown string to put inside Scalar's description box
            var sb = new StringBuilder(operation.Description);
            if (sb.Length > 0) sb.AppendLine("\n");

            sb.AppendLine("### 🔐 Authorization Requirements");

            if (roles.Count > 0)
            {
                sb.AppendLine($"- **Required Roles:** `{string.Join(", ", roles)}`");
            }
            if (policies.Count > 0)
            {
                sb.AppendLine($"- **Required Policies:** `{string.Join(", ", policies)}`");
            }

            operation.Description = sb.ToString();
        }

        return Task.CompletedTask;
    }


    /// <summary>
    /// Helper to dynamically declare the RFC 7807 ProblemDetails schema in the OpenAPI document components.
    /// </summary>
    private IOpenApiSchema GetOrCreateProblemDetailsSchema(OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        const string schemaKey = "ProblemDetails";

        if (!document.Components.Schemas.ContainsKey(schemaKey))
        {
            // FIX: Use JsonSchemaType enum values instead of strings
            var schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Title = "ProblemDetails",
                Description = "A machine-readable format for specifying errors in HTTP responses based on RFC 7807.",
                Properties = new Dictionary<string, IOpenApiSchema>(),
            };

            // FIX: Use JsonSchemaType for individual property types as well
            schema.Properties["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri", Description = "A URI reference that identifies the problem type." };
            schema.Properties["title"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "A short, human-readable summary of the problem type." };
            schema.Properties["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", Description = "The HTTP status code generated by the origin server for this occurrence of the problem." };
            schema.Properties["detail"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "A human-readable explanation specific to this occurrence of the problem." };
            schema.Properties["instance"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri", Description = "A URI reference that identifies the specific occurrence of the problem." };

            document.Components.Schemas[schemaKey] = schema;
        }

        return new OpenApiSchemaReference(schemaKey, document);
    }


}
