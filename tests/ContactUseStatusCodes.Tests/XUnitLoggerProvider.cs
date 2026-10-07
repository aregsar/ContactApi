using Microsoft.Extensions.Logging;

namespace ContactUseStatusCodes.Tests;


// public sealed class XUnitLoggingProvider : ILoggerProvider
// {
//     // The framework calls this and passes the category name automatically
//     public ILogger CreateLogger(string categoryName) => new XUnitLogger(categoryName);

//     public void Dispose() { }
// }

// Explicitly pass the active test's output helper to bypass background thread isolation
public sealed class XUnitLoggingProvider(ITestOutputHelper output) : ILoggerProvider
{
    //private readonly ITestOutputHelper _output;

    // Explicitly pass the active test's output helper to bypass background thread isolation
    // public XUnitLoggingProvider(ITestOutputHelper output)
    // {
    //     _output = output;
    // }

    public ILogger CreateLogger(string categoryName) => new XUnitLogger(categoryName, output);
    public void Dispose() { }
}

// 2. THE LOGGER (Created for each category)
public sealed class XUnitLogger(string categoryName, ITestOutputHelper output) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        //var output = TestContext.Current.TestOutputHelper;
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
