using Microsoft.Extensions.Logging;

namespace ContactUseStatusCodes.Tests;


// 1. THE PROVIDER (Registered via logging.AddProvider)
public sealed class XUnitLoggingProvider : ILoggerProvider
{
    // The framework calls this and passes the category name automatically
    public ILogger CreateLogger(string categoryName) => new XUnitLogger(categoryName);

    public void Dispose() { }
}

// 2. THE LOGGER (Created for each category)
public sealed class XUnitLogger(string categoryName) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var output = TestContext.Current.TestOutputHelper;
        if (output == null) return;

        try
        {
            var message = formatter(state, exception);
            output.WriteLine($"[{logLevel}] [{categoryName}] {message}");


            if (exception != null)
            {
                output.WriteLine(exception.ToString());
            }
        }
        catch
        {
            // Prevent test runner from crashing if logging happens post-test disposal
        }

    }
}
