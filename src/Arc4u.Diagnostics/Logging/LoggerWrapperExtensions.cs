using Arc4u.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggerWrapperExtensions
{
    public static LoggerWrapper<T> Add<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        logger.ThrowIfDisposed();
        logger.AdditionalFields[ValidateKey(key)] = value;
        return logger;
    }

    public static LoggerWrapper<T> AddIf<T>(this LoggerWrapper<T> logger, bool condition, string key, object? value)
    {
        logger.ThrowIfDisposed();
        if (condition)
        {
            logger.AdditionalFields[ValidateKey(key)] = value;
        }
        return logger;
    }

    public static LoggerWrapper<T> AddIfNotExist<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        if (value == null || logger.AdditionalFields.ContainsKey(key))
        {
            return logger;
        }

        logger.AdditionalFields[ValidateKey(key)] = value;
        return logger;
    }

    public static LoggerWrapper<T> AddOrReplace<T>(this LoggerWrapper<T> logger, string key, object? value)
    {
        logger.ThrowIfDisposed();
        logger.AdditionalFields[ValidateKey(key)] = value;
        return logger;
    }

    public static LoggerWrapper<T> AddOrReplaceIf<T>(this LoggerWrapper<T> logger, bool condition, string key, object? value)
    {
        logger.ThrowIfDisposed();

        if (condition)
        {
            logger.AdditionalFields[ValidateKey(key)] = value;
        }
        return logger;
    }

    public static LoggerWrapper<T> AddStackTrace<T>(this LoggerWrapper<T> logger)
    {
        logger.IncludeStackTrace = true;
        return logger;
    }

    private static string ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        return key switch
        {
            LoggingConstants.ActivityId or
            LoggingConstants.Application or
            LoggingConstants.Category or
            LoggingConstants.Class or
            LoggingConstants.Identity or
            LoggingConstants.MethodName or
            LoggingConstants.ProcessId or
            LoggingConstants.Stacktrace or
            LoggingConstants.ThreadId =>
            throw new ReservedLoggingKeyException(key),
            _ => key,
        };
    }
}
