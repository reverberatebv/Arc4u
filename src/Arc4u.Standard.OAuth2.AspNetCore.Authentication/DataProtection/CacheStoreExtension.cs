using System.Diagnostics.CodeAnalysis;
using Arc4u.Caching;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.DataProtection;
public static class CacheStoreExtension
{
    public static IDataProtectionBuilder PersistKeysToCache(this IDataProtectionBuilder builder, Action<CacheStoreOption> option)
    {
        var validate = new CacheStoreOption();
        option(validate);

        ArgumentNullException.ThrowIfNull(validate.CacheKey);

        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
        {
            var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            var cacheContext = services.GetRequiredService<ICacheContext>();
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new CacheStore(cacheContext, loggerFactory, validate.CacheKey, validate.CacheName);
            });
        });

        return builder;
    }

    public static IDataProtectionBuilder PersistKeysToCache(this IDataProtectionBuilder builder, [DisallowNull] IConfiguration configuration, [DisallowNull] string configSectionName = "DataProtectionStore")
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return PersistKeysToCache(builder, PrepareAction(configuration, configSectionName));
    }

    internal static Action<CacheStoreOption> PrepareAction(IConfiguration configuration, string configSectionName)
    {
        var section = configuration.GetSection(configSectionName);
        if (!section.Exists())
        {
            throw new KeyNotFoundException($"A section with name {configSectionName} doesn't exist.");
        }

        var storeInfo = section.Get<CacheStoreOption>();
        if (storeInfo == null)
        {
            throw new InvalidCastException($"Retrieving the cache data protection store info from section {configSectionName} is impossible.");
        }

        if (storeInfo.CacheKey is null && storeInfo.CacheName is null)
        {
            throw new InvalidCastException($"Retrieving the CacheKey or CacheName data protection store info from section {configSectionName} is impossible.");
        }

        void OptionsFiller(CacheStoreOption option)
        {
            ArgumentNullException.ThrowIfNull(option);

            option.CacheKey = storeInfo.CacheKey ?? throw new InvalidCastException($"CacheKey from section {configSectionName} is null.");
            option.CacheName = storeInfo.CacheName;
        }

        return OptionsFiller;
    }
}
