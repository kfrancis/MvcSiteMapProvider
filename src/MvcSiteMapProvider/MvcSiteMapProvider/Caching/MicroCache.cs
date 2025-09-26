using System;
using System.Collections.Generic;
using System.Threading;

namespace MvcSiteMapProvider.Caching
{
    public class MicroCache<T>
        : IMicroCache<T>
    {
        protected MicroCache(
            ICacheProvider<T> cacheProvider
            )
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            this.cacheProvider.ItemRemoved += cacheProvider_ItemRemoved;
        }

        private readonly ICacheProvider<T> cacheProvider;
        private readonly ReaderWriterLockSlim synclock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public event EventHandler<MicroCacheItemRemovedEventArgs<T>>? ItemRemoved;

        public bool Contains(string key)
        {
            synclock.EnterReadLock();
            try { return this.cacheProvider.Contains(key); }
            finally { synclock.ExitReadLock(); }
        }

        public T? GetOrAdd(string key, Func<T> loadFunction, Func<ICacheDetails> getCacheDetailsFunction)
        {
            LazyLock lazy; bool success;
            synclock.EnterReadLock();
            try { success = this.cacheProvider.TryGetValue(key, out lazy); }
            finally { synclock.ExitReadLock(); }

            if (!success)
            {
                synclock.EnterWriteLock();
                try
                {
                    if (!this.cacheProvider.TryGetValue(key, out lazy))
                    {
                        lazy = new LazyLock();
                        var cacheDetails = getCacheDetailsFunction();
                        this.cacheProvider.Add(key, lazy, cacheDetails);
                    }
                }
                finally { synclock.ExitWriteLock(); }
            }
            return lazy.Get(loadFunction);
        }

        public void Remove(string key)
        {
            synclock.EnterWriteLock();
            try { this.cacheProvider.Remove(key); }
            finally { synclock.ExitWriteLock(); }
        }

        protected virtual void cacheProvider_ItemRemoved(object sender, MicroCacheItemRemovedEventArgs<T?> e)
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
}
