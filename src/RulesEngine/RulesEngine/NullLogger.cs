using System;
using Microsoft.Extensions.Logging;

namespace RulesEngine
{
    public class NullLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullScope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        { }
    }

    public class NullScope : IDisposable
    {
        public void Dispose()
        { }
    }
}