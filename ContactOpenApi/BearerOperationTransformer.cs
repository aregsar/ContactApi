using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;


public sealed class BearerOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. Check if the endpoint allows anonymous access
        var hasAllowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (hasAllowAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        // 2. Use the new v3.x Reference class directly
        // Pass the matching ID "BearerAuth" and the active Document context
        var securitySchemeRef = new OpenApiSecuritySchemeReference("BearerAuth", context.Document);

        var requirement = new OpenApiSecurityRequirement
        {
            [securitySchemeRef] = []
        };

        operation.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
