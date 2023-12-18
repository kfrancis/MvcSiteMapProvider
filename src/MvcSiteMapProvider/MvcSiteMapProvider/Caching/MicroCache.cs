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
        public MicroCache(
            ICacheProvider<T> cacheProvider
            )
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));

            // Attach our event so we can receive notifications when objects are removed
            this.cacheProvider.ItemRemoved += cacheProvider_ItemRemoved;
        }

        protected readonly ICacheProvider<T> cacheProvider;
        private ReaderWriterLockSlim synclock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        #region IMicroCache<T> Members

        public event EventHandler<MicroCacheItemRemovedEventArgs<T>> ItemRemoved;

        public bool Contains(string key)
        {
            synclock.EnterReadLock();
            try
            {
                return cacheProvider.Contains(key);
            }
            finally
            {
                synclock.ExitReadLock();
            }
        }

        public T GetOrAdd(string key, Func<T> loadFunction, Func<ICacheDetails> getCacheDetailsFunction)
        {
            LazyLock lazy;
            bool success;

            synclock.EnterReadLock();
            try
            {
                success = cacheProvider.TryGetValue(key, out lazy);
            }
            finally
            {
                synclock.ExitReadLock();
            }

            if (!success)
            {
                synclock.EnterWriteLock();
                try
                {
                    if (!cacheProvider.TryGetValue(key, out lazy))
                    {
                        lazy = new LazyLock();
                        var cacheDetails = getCacheDetailsFunction();
                        cacheProvider.Add(key, lazy, cacheDetails);
                    }
                }
                finally
                {
                    synclock.ExitWriteLock();
                }
            }

            return lazy.Get(loadFunction);
        }

        public void Remove(string key)
        {
            synclock.EnterWriteLock();
            try
            {
                cacheProvider.Remove(key);
            }
            finally
            {
                synclock.ExitWriteLock();
            }
        }

        #endregion IMicroCache<T> Members

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
            if (ItemRemoved != null)
            {
                ItemRemoved(this, e);
            }
        }
    }
}