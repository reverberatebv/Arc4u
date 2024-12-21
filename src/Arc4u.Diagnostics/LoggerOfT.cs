using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.Diagnostics;

class Logger<T>(ILoggerFactory loggerFactory, IServiceProvider serviceProvider) : ILogger<T>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<T>();
    private IAddPropertiesToLog? _addPropertiesToLog;
    private bool _propertiesInitialized;

    IDisposable? ILogger.BeginScope<TState>(TState state) => _logger.BeginScope(state);

    bool ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> pairs)
        {
            var properties = pairs as IDictionary<string, object>;

            if (null == properties)
            {
                properties = new Dictionary<string, object>();
                foreach (var pair in pairs)
                {
                    properties.Add(pair.Key, pair.Value);
                }
            }

            if (!_propertiesInitialized)
            {
                try
                {
                    _addPropertiesToLog = serviceProvider.GetService<IAddPropertiesToLog>();
                }
                catch (Exception)
                {
                }
                _propertiesInitialized = true;
            }

            foreach (var property in _addPropertiesToLog?.GetProperties() ?? new Dictionary<string, object>())
            {
                properties.AddIfNotExist(property.Key, property.Value);
            }

        }

        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
