using System;
using System.Collections.Generic;
using System.Threading;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A lightweight cache that ensures thread safety when loading items by using a callback that
    /// gets called exactly 1 time.
    /// </summary>
    /// <typeparam name="T">The type of object to cache.</typeparam>
    /// <remarks>
    /// Caching strategy inspired by this post:
    /// http://www.superstarcoders.com/blogs/posts/micro-caching-in-asp-net.aspx
    /// </remarks>
    public class MicroCache<T>
        : IMicroCache<T>
    {
        protected readonly ICacheProvider<T> CacheProvider;

        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public MicroCache(ICacheProvider<T> cacheProvider)
        {
            CacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));

            // Attach our event so we can receive notifications when objects are removed
            CacheProvider.ItemRemoved += cacheProvider_ItemRemoved;
        }

        public event EventHandler<MicroCacheItemRemovedEventArgs<T>> ItemRemoved;

        public bool Contains(string key)
        {
            _synclock.EnterReadLock();
            try
            {
                return CacheProvider.Contains(key);
            }
            finally
            {
                _synclock.ExitReadLock();
            }
        }

        public T GetOrAdd(string key, Func<T> loadFunction, Func<ICacheDetails> getCacheDetailsFunction)
        {
            LazyLock lazy;
            bool success;

            _synclock.EnterReadLock();
            try
            {
                success = CacheProvider.TryGetValue(key, out lazy);
            }
            finally
            {
                _synclock.ExitReadLock();
            }

            if (!success)
            {
                _synclock.EnterWriteLock();
                try
                {
                    if (!CacheProvider.TryGetValue(key, out lazy))
                    {
                        lazy = new LazyLock();
                        var cacheDetails = getCacheDetailsFunction();
                        CacheProvider.Add(key, lazy, cacheDetails);
                    }
                }
                finally
                {
                    _synclock.ExitWriteLock();
                }
            }

            return lazy.Get(loadFunction);
        }

        public void Remove(string key)
        {
            _synclock.EnterWriteLock();
            try
            {
                CacheProvider.Remove(key);
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
        }

        protected virtual void cacheProvider_ItemRemoved(object sender, MicroCacheItemRemovedEventArgs<T> e)
        {
            // Skip the event if the item is null, empty, or otherwise a default value,
            // since nothing was actually put in the cache, so nothing was removed
            if (!EqualityComparer<T>.Default.Equals(e.Item, default))
            {
                // Cascade the event
                OnCacheItemRemoved(e);
            }
        }

        protected virtual void OnCacheItemRemoved(MicroCacheItemRemovedEventArgs<T> e)
        {
            ItemRemoved?.Invoke(this, e);
        }
    }
}
