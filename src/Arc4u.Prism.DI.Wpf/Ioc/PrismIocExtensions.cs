using Prism.Ioc;
using IContainerRegistry = Prism.Ioc.IContainerRegistry;

namespace Prism.DI.Ioc;

public static class PrismIocExtensions
{
    public static IServiceProvider GetContainer(this IContainerProvider containerProvider)
    {
        return ((IContainerExtension<IServiceProvider>)containerProvider).Instance;
    }

    public static IServiceProvider GetContainer(this IContainerRegistry containerRegistry)
    {
        return ((IContainerExtension<IServiceProvider>)containerRegistry).Instance;
    }
}
