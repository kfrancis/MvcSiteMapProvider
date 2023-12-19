using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Web;
using System.Web.Caching;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A cache provider that uses the <see cref="T:System.Web.HttpContext.Current.Cache"/> instance to
    /// cache items that are added.
    /// </summary>
    /// <typeparam name="T">The type of item that will be stored in the cache.</typeparam>
    public class AspNetCacheProvider<T>
        : ICacheProvider<T>
    {
        private readonly IMvcContextFactory _mvcContextFactory;

        public AspNetCacheProvider(
                    IMvcContextFactory mvcContextFactory
            )
        {
            _mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        }

        public event EventHandler<MicroCacheItemRemovedEventArgs<T>> ItemRemoved;

        protected HttpContextBase Context
        {
            get
            {
                return _mvcContextFactory.CreateHttpContext();
            }
        }

        public void Add(string key, LazyLock item, ICacheDetails cacheDetails)
        {
            var absolute = System.Web.Caching.Cache.NoAbsoluteExpiration;
            var sliding = System.Web.Caching.Cache.NoSlidingExpiration;
            if (IsTimespanSet(cacheDetails.AbsoluteCacheExpiration))
            {
                absolute = DateTime.UtcNow.Add(cacheDetails.AbsoluteCacheExpiration);
            }
            else if (IsTimespanSet(cacheDetails.SlidingCacheExpiration))
            {
                sliding = cacheDetails.SlidingCacheExpiration;
            }
            var dependency = (CacheDependency)cacheDetails.CacheDependency.Dependency;

            Context.Cache.Insert(key, item, dependency, absolute, sliding, CacheItemPriority.NotRemovable, OnItemRemoved);
        }

        public bool Contains(string key)
        {
            return Context.Cache[key] != null;
        }

        public LazyLock Get(string key)
        {
            return (LazyLock)Context.Cache.Get(key);
        }

        public void Remove(string key)
        {
            Context.Cache.Remove(key);
        }

        public bool TryGetValue(string key, out LazyLock value)
        {
            value = Get(key);
            return value != null;
        }

        protected virtual void OnCacheItemRemoved(MicroCacheItemRemovedEventArgs<T> e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        /// <summary>
        /// This method is called when an item has been removed from the cache.
        /// </summary>
        /// <param name="key">Cached item key.</param>
        /// <param name="item">Cached item.</param>
        /// <param name="reason">Reason the cached item was removed.</param>
        protected virtual void OnItemRemoved(string key, object item, CacheItemRemovedReason reason)
        {
            var args = new MicroCacheItemRemovedEventArgs<T>(key, ((LazyLock)item).Get<T>(null));
            OnCacheItemRemoved(args);
        }

        private bool IsTimespanSet(TimeSpan timeSpan)
        {
            return !timeSpan.Equals(TimeSpan.MinValue);
        }
    }
}
