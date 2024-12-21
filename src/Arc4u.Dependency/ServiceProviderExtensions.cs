using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency;
public static class ServiceProviderExtensions
{
    public static bool TryGetService<T>(this IServiceProvider provider, out T? service)
    {
        try
        {
            service = provider.GetService<T>();
        }
        catch (Exception)
        {
            service = default;
        }
        return null != service;
    }

    [Obsolete("Use TryGetService instead.")]
    public static bool TryResolve<T>(this IServiceProvider provider, out T? service)
    {
        return TryGetService(provider, out service);
    }

    public static bool TryGetService(this IServiceProvider provider, Type type, string name, out object? value)
    {
        try
        {
            value = provider.GetRequiredKeyedService(type, name);
            return value is not null;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    [Obsolete("Use TryGetService instead.")]
    public static bool TryResolve(this IServiceProvider provider, Type type, string name, out object? value)
    {
        return TryGetService(provider, type, name, out value);
    }

    public static bool TryGetService<T>(this IServiceProvider provider, string name, out T? service)
    {
        try
        {
            service = provider.GetKeyedService<T>(name);

        }
        catch (Exception)
        {
            service = default;
        }
        return null != service;
    }

    [Obsolete("Use TryGetService instead.")]
    public static bool TryResolve<T>(this IServiceProvider provider, string name, out T? service)
    {
        return TryGetService(provider, name, out service);
    }
}
