using System;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// Container for passing caching instructions around as a group.
    /// </summary>
    public class CacheDetails
        : ICacheDetails
    {
        public CacheDetails(
            TimeSpan absoluteCacheExpiration,
            TimeSpan slidingCacheExpiration,
            ICacheDependency cacheDependency
            )
        {
            if (absoluteCacheExpiration == null)
                throw new ArgumentNullException(nameof(absoluteCacheExpiration));
            if (slidingCacheExpiration == null)
                throw new ArgumentNullException(nameof(slidingCacheExpiration));
            this.absoluteCacheExpiration = absoluteCacheExpiration;
            this.slidingCacheExpiration = slidingCacheExpiration;
            this.cacheDependency = cacheDependency ?? throw new ArgumentNullException(nameof(cacheDependency));
        }

        protected readonly TimeSpan absoluteCacheExpiration;
        protected readonly TimeSpan slidingCacheExpiration;
        protected readonly ICacheDependency cacheDependency;

        #region ICacheDetails Members

        public TimeSpan AbsoluteCacheExpiration
        {
            get { return absoluteCacheExpiration; }
        }

        public TimeSpan SlidingCacheExpiration
        {
            get { return slidingCacheExpiration; }
        }

        public ICacheDependency CacheDependency
        {
            get { return cacheDependency; }
        }

        #endregion ICacheDetails Members
    }
}