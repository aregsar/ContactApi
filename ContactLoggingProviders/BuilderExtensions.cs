namespace Microsoft.Extensions.Hosting;

public static class BuilderExtensions
{
    public static TBuilder AddLoggingProviders<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.ClearProviders();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        return builder;
    }
}