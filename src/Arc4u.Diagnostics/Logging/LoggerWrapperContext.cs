using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Diagnostics.Logging;
internal class LoggerWrapperContext
{
    private static IServiceScopeFactory? _scopeFactory;

    public static void Initialize(IServiceProvider serviceProvider)
        => _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

    public static IServiceScope CreateScope()
    {
        if (_scopeFactory == null)
        {
            throw new InvalidOperationException("LoggerContext not initialized");
        }
        return _scopeFactory.CreateScope();
    }
}
