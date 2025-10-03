using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     A cache provider that uses an <see cref="System.Runtime.Caching.ObjectCache" /> instance to cache items.
/// </summary>
/// <typeparam name="T">The type of item that will be stored in the cache.</typeparam>
public class RuntimeCacheProvider<T> : ICacheProvider<T>
{
    private readonly ObjectCache _cache;

    public RuntimeCacheProvider(ObjectCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public event EventHandler<MicroCacheItemRemovedEventArgs<T?>>? ItemRemoved;

    public bool Contains(string key)
    {
        return _cache.Contains(key);
    }

    public LazyLock? Get(string key)
    {
        return (LazyLock)_cache.Get(key);
    }

    public bool TryGetValue(string key, out LazyLock? value)
    {
        value = Get(key);
        return value != null;
    }

    public void Add(string key, LazyLock item, ICacheDetails cacheDetails)
    {
        var policy = new CacheItemPolicy
        {
            // Timeout
            Priority = CacheItemPriority.NotRemovable
        };

        if (IsTimespanSet(cacheDetails.AbsoluteCacheExpiration))
        {
            policy.AbsoluteExpiration = DateTimeOffset.Now.Add(cacheDetails.AbsoluteCacheExpiration);
        }
        else if (IsTimespanSet(cacheDetails.SlidingCacheExpiration))
        {
            policy.SlidingExpiration = cacheDetails.SlidingCacheExpiration;
        }

        // Dependencies
        if (cacheDetails.CacheDependency.Dependency is IList<ChangeMonitor> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                policy.ChangeMonitors.Add(dependency);
            }
        }

        // Callback
        policy.RemovedCallback = CacheItemRemoved;

        _cache.Add(key, item, policy);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    private static bool IsTimespanSet(TimeSpan timeSpan)
    {
        return !timeSpan.Equals(TimeSpan.MinValue);
    }

    protected virtual void CacheItemRemoved(CacheEntryRemovedArguments arguments)
    {
        var item = arguments.CacheItem;
        var args = new MicroCacheItemRemovedEventArgs<T?>(item.Key, ((LazyLock)item.Value).Get<T>(null));
        OnCacheItemRemoved(args);
    }

    protected virtual void OnCacheItemRemoved(MicroCacheItemRemovedEventArgs<T?> e)
    {
        var handler = ItemRemoved;
        handler?.Invoke(this, e);
    }
}
