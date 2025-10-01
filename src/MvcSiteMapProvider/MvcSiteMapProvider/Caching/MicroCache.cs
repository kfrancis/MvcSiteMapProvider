using System;
using System.Collections.Generic;
using System.Threading;

namespace MvcSiteMapProvider.Caching;

public class MicroCache<T>
    : IMicroCache<T>
{
    protected MicroCache(
        ICacheProvider<T> cacheProvider
    )
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _cacheProvider.ItemRemoved += CacheProviderItemRemoved;
    }

    private readonly ICacheProvider<T> _cacheProvider;
    private readonly ReaderWriterLockSlim _synclock = new(LockRecursionPolicy.NoRecursion);

    public event EventHandler<MicroCacheItemRemovedEventArgs<T>>? ItemRemoved;

    public bool Contains(string key)
    {
        _synclock.EnterReadLock();
        try { return _cacheProvider.Contains(key); }
        finally { _synclock.ExitReadLock(); }
    }

    public T? GetOrAdd(string key, Func<T> loadFunction, Func<ICacheDetails> getCacheDetailsFunction)
    {
        LazyLock? lazy; bool success;
        _synclock.EnterReadLock();
        try { success = _cacheProvider.TryGetValue(key, out lazy); }
        finally { _synclock.ExitReadLock(); }

        if (success && lazy != null)
        {
            return lazy.Get(loadFunction);
        }

        _synclock.EnterWriteLock();
        try
        {
            if (!_cacheProvider.TryGetValue(key, out lazy))
            {
                lazy = new LazyLock();
                var cacheDetails = getCacheDetailsFunction();
                _cacheProvider.Add(key, lazy, cacheDetails);
            }
        }
        finally { _synclock.ExitWriteLock(); }

        return lazy != null ? lazy.Get(loadFunction) : default;
    }

    public void Remove(string key)
    {
        _synclock.EnterWriteLock();
        try { _cacheProvider.Remove(key); }
        finally { _synclock.ExitWriteLock(); }
    }

    protected virtual void CacheProviderItemRemoved(object sender, MicroCacheItemRemovedEventArgs<T?>? e)
    {
        // Skip raising event when item is default(T) (also null for reference types)
        if (e == null || EqualityComparer<T?>.Default.Equals(e.Item, default))
            return;
        OnCacheItemRemoved(new MicroCacheItemRemovedEventArgs<T>(e.Key, e.Item!));
    }

    protected virtual void OnCacheItemRemoved(MicroCacheItemRemovedEventArgs<T> e)
    {
        ItemRemoved?.Invoke(this, e);
    }
}
