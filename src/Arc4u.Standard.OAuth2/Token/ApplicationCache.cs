using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Token;

[Export(typeof(ITokenCache)), Shared]
public class ApplicationCache : ITokenCache
{
    /// <summary>
    /// Read the cache used to store the tokens! If nothing is identified, Default is used!
    /// </summary>
    public ApplicationCache(ICacheContext cacheContext, ICacheHelper cacheHelper, ILogger logger, IOptions<TokenCacheOptions> options)
    {
        _logger = logger;
        _cacheContext = cacheContext;
        _cache = cacheHelper.GetCache();
        _tokenCacheOptions = options.Value;
    }

    private readonly ICache _cache;
    private readonly ICacheContext _cacheContext;
    private readonly ILogger _logger;
    private readonly TokenCacheOptions _tokenCacheOptions;

    /// <summary>
    /// Remove at the same time the token and the extra claims added to the cache via a call to an implementation of the IClaimsFiller...
    /// </summary>
    /// <param name="id"></param>
    public void DeleteItem(string id)
    {
        _logger.Technical().From<ApplicationCache>().System($"Deleting information from the token cache for the id: {id}.").Log();
        _cache.Remove(ApplicationCache.GetKey(id));
        _logger.Technical().From<ApplicationCache>().System($"Deleted information from the token cache for the id: {id}.").Log();
    }

    public void Put<T>(string key, T data)
    {
        if (null == data)
        {
            _logger.Technical().From<ApplicationCache>().System("A null token data information was provided to the cache. We skip this data from the cache.");
            return;
        }

        _logger.Technical().From<ApplicationCache>().System($"Adding token data information to the cache: {key}.").Log();
        _cache.Put(ApplicationCache.GetKey(key), _tokenCacheOptions.MaxTime, data);
        _logger.Technical().From<ApplicationCache>().System($"Added token data information to the cache: {key}.").Log();
    }

    public T? Get<T>(string id)
    {
        _logger.Technical().From<ApplicationCache>().System($"Retrieve token information for user: {id}.").Log();
        var data = _cache.Get<T>(ApplicationCache.GetKey(id));

        if (null == data)
        {
            _logger.Technical().From<ApplicationCache>().System($"The data in cache is null for user: {id}.").Log();
        }

        return data;
    }

    public IEnumerable<byte[]> GetAll()
    {
        _logger.Technical().From<ApplicationCache>().Warning("Geting all data from the token cache is not implemented.").Log();

        return [];
    }

    private static string GetKey(string id)
    {
        return (id + "_TokenCache").ToLowerInvariant();
    }

}
