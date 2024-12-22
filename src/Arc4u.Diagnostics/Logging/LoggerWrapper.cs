using System.Reflection;
using System.Text;
using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public sealed class LoggerWrapper<T> : ILogger<T>
{
    private readonly ILogger _logger;
    private readonly MessageCategory _category;
    private readonly Dictionary<string, object?> _additionalFields = [];
    private readonly Type? _contextType;
    private readonly string _caller = string.Empty;
    private bool _disposed;
    private bool _includeStackTrace;

    internal Dictionary<string, object?> AdditionalFields => _additionalFields;
    internal bool IncludeStackTrace { get => _includeStackTrace; set => _includeStackTrace = value; }

    internal static int ProcessId
    {
        get
        {
            try
            {
                return Environment.ProcessId;
            }
            catch (PlatformNotSupportedException)
            {
                return -1;
            }
        }
    }

    public LoggerWrapper(ILoggerFactory loggerFactory) =>
        _logger = loggerFactory.CreateLogger<T>();

    internal LoggerWrapper(
        ILogger logger,
        MessageCategory category,
        Type? contextType = null,
        string caller = "")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _category = category;
        _contextType = contextType;
        _caller = caller;
    }

    private static class LoggerMessages
    {
        private static readonly EventId TechnicalEventId = new(1000, "Technical");
        private static readonly EventId BusinessEventId = new(2000, "Business");
        private static readonly EventId MonitoringEventId = new(3000, "Monitoring");

        public static readonly Func<LogLevel, Action<ILogger, string, string, object, string, Exception?>> TechnicalLog =
            level => Microsoft.Extensions.Logging.LoggerMessage.Define<string, string, object, string>(
                level,
                TechnicalEventId,
                "{Category} [{Context}] [{@State}] {Message}");

        public static readonly Func<LogLevel, Action<ILogger, string, string, object, string, Exception?>> BusinessLog =
            level => Microsoft.Extensions.Logging.LoggerMessage.Define<string, string, object, string>(
                level,
                BusinessEventId,
                "{Category} [{Context}] [{@State}] {Message}");

        public static readonly Func<LogLevel, Action<ILogger, string, string, object, string, Exception?>> MonitoringLog =
            level => Microsoft.Extensions.Logging.LoggerMessage.Define<string, string, object, string>(
                level,
                MonitoringEventId,
                "{Category} [{Context}] [{@State}] {Message}");
    }

    private void Log(LogLevel level, string? message, Exception? exception = null)
    {
        ThrowIfDisposed();

        if (!IsEnabled(level))
        {
            return;
        }

        try
        {
            var properties = AddAdditionalProperties();
            if (IncludeStackTrace)
            {
                properties.AddIfNotExist(LoggingConstants.Stacktrace, exception?.StackTrace ?? Environment.StackTrace);
            }

            if (exception is not null)
            {
                //properties.AddIfNotExist(LoggingConstants.UnwrappedException, exception.ToFormattedstring());
            }

            properties.AddIfNotExist(LoggingConstants.MethodName, _caller);
            properties.AddIfNotExist(LoggingConstants.Class, _contextType?.FullName ?? nameof(_contextType));
            properties.AddIfNotExist(LoggingConstants.Category, (short)_category);
            properties.AddIfNotExist(LoggingConstants.Application, Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown App");
            properties.AddIfNotExist(LoggingConstants.ThreadId, Environment.CurrentManagedThreadId);
            properties.AddIfNotExist(LoggingConstants.ProcessId, ProcessId);

            var enrichedState = new
            {
                Caller = _caller,
                TimeStamp = DateTime.UtcNow,
                AdditionalFields = properties,
                Level = level
            };

            var logAction = _category switch
            {
                MessageCategory.Technical => LoggerMessages.TechnicalLog(level),
                MessageCategory.Business => LoggerMessages.BusinessLog(level),
                MessageCategory.Monitoring => LoggerMessages.MonitoringLog(level),
                _ => LoggerMessages.TechnicalLog(level)
            };

            logAction(
                _logger,
                _category.ToString(),
                _contextType?.Name ?? "General",
                enrichedState,
                message ?? string.Empty,
                exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log message with property providers");
            _logger.Log(level, exception, message ?? string.Empty);
        }
    }

    internal void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    private Dictionary<string, object?> AddAdditionalProperties()
    {
        Dictionary<string, object?> properties;

        try
        {
            using var scope = LoggerWrapperContext.CreateScope();
            var providers = scope.ServiceProvider
                .GetServices<IAddPropertiesToLog>();

            properties = new Dictionary<string, object?>(AdditionalFields);
            var definedProperties = providers.Select(x => x.GetProperties()).SelectMany(x => x);
            if (definedProperties != null)
            {
                foreach (var property in definedProperties)
                {
                    if (property.Value != null)
                    {
                        this.AddIfNotExist(property.Key, property.Value);
                    }
                }

                return properties;
            }

            return new Dictionary<string, object?>(AdditionalFields);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting property providers. Logging without additional properties.");
            return new Dictionary<string, object?>(AdditionalFields);
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine("Log<TState> is called.");

        if (state is IEnumerable<KeyValuePair<string, object>> pairs)
        {
            foreach (var pair in pairs)
            {
                AdditionalFields.AddOrReplace(pair.Key, pair.Value);
            }
        }

        Log(logLevel, formatter(state, exception), exception);
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
}

internal static class DumpException
{
    internal static string ToFormattedstring(this Exception exception)
    {
        var messages = exception
            .GetAllExceptions()
            .Where(e => !string.IsNullOrWhiteSpace(e.Message))
            .Select(e => e.GetType().FullName + " : " + e.Message.Trim());
        var sb = new StringBuilder();
        var i = 0;
        foreach (var message in messages)
        {
            sb.Append("".PadLeft(i * 4));
            sb.Append("|---".PadLeft(i++ > 0 ? 4 : 0));
            sb.AppendLine(message);
        }

        return sb.ToString();
    }

    private static IEnumerable<Exception> GetAllExceptions(this Exception exception)
    {
        yield return exception;

        if (exception is AggregateException aggrEx)
        {
            foreach (var innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
            {
                yield return innerEx;
            }
        }
        else if (exception.InnerException != null)
        {
            foreach (var innerEx in exception.InnerException.GetAllExceptions())
            {
                yield return innerEx;
            }
        }
    }
}

