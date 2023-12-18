using MvcSiteMapProvider.Caching;
using System;

namespace MvcSiteMapProvider.Loader
{
    /// <summary>
    /// <see cref="T:MvcSiteMapProvider.Loader.SiteMapLoader"/> is responsible for loading or unloading
    /// an <see cref="T:MvcSitemapProvider.ISiteMap"/> instance from the cache.
    /// </summary>
    public class SiteMapLoader
        : ISiteMapLoader
    {
        protected readonly ISiteMapCache siteMapCache;

        protected readonly ISiteMapCacheKeyGenerator siteMapCacheKeyGenerator;

        protected readonly ISiteMapCreator siteMapCreator;

        public SiteMapLoader(
                                    ISiteMapCache siteMapCache,
            ISiteMapCacheKeyGenerator siteMapCacheKeyGenerator,
            ISiteMapCreator siteMapCreator
            )
        {
            this.siteMapCache = siteMapCache ?? throw new ArgumentNullException(nameof(siteMapCache));
            this.siteMapCacheKeyGenerator = siteMapCacheKeyGenerator ?? throw new ArgumentNullException(nameof(siteMapCacheKeyGenerator));
            this.siteMapCreator = siteMapCreator ?? throw new ArgumentNullException(nameof(siteMapCreator));
        }

        public virtual ISiteMap GetSiteMap()
        {
            return GetSiteMap(null);
        }

        public virtual ISiteMap GetSiteMap(string siteMapCacheKey)
        {
            if (string.IsNullOrEmpty(siteMapCacheKey))
            {
                siteMapCacheKey = siteMapCacheKeyGenerator.GenerateKey();
            }
            return siteMapCache.GetOrAdd(
                siteMapCacheKey,
                () => siteMapCreator.CreateSiteMap(siteMapCacheKey),
                () => siteMapCreator.GetCacheDetails(siteMapCacheKey));
        }

        public virtual void ReleaseSiteMap()
        {
            ReleaseSiteMap(null);
        }

        public virtual void ReleaseSiteMap(string siteMapCacheKey)
        {
            if (string.IsNullOrEmpty(siteMapCacheKey))
            {
                siteMapCacheKey = siteMapCacheKeyGenerator.GenerateKey();
            }
            siteMapCache.Remove(siteMapCacheKey);
        }
    }
}