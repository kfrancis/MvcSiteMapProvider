using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using System;

namespace MvcSiteMapProvider.Loader
{
    /// <summary>
    /// Builds a specific <see cref="T:MvcSiteMapProvider.ISiteMap"/> instance based on a cache key.
    /// </summary>
    public class SiteMapCreator
        : ISiteMapCreator
    {
        public SiteMapCreator(
            ISiteMapCacheKeyToBuilderSetMapper siteMapCacheKeyToBuilderSetMapper,
            ISiteMapBuilderSetStrategy siteMapBuilderSetStrategy,
            ISiteMapFactory siteMapFactory
            )
        {
            this.siteMapCacheKeyToBuilderSetMapper = siteMapCacheKeyToBuilderSetMapper ?? throw new ArgumentNullException(nameof(siteMapCacheKeyToBuilderSetMapper));
            this.siteMapBuilderSetStrategy = siteMapBuilderSetStrategy ?? throw new ArgumentNullException(nameof(siteMapBuilderSetStrategy));
            this.siteMapFactory = siteMapFactory ?? throw new ArgumentNullException(nameof(siteMapFactory));
        }

        protected readonly ISiteMapCacheKeyToBuilderSetMapper siteMapCacheKeyToBuilderSetMapper;
        protected readonly ISiteMapBuilderSetStrategy siteMapBuilderSetStrategy;
        protected readonly ISiteMapFactory siteMapFactory;

        #region ISiteMapCreator Members

        public virtual ISiteMap CreateSiteMap(string siteMapCacheKey)
        {
            if (string.IsNullOrEmpty(siteMapCacheKey))
            {
                throw new ArgumentNullException(nameof(siteMapCacheKey));
            }

            var builderSet = GetBuilderSet(siteMapCacheKey);
            var siteMap = siteMapFactory.Create(builderSet.Builder, builderSet);
            siteMap.BuildSiteMap();

            return siteMap;
        }

        public virtual ICacheDetails GetCacheDetails(string siteMapCacheKey)
        {
            var builderSet = GetBuilderSet(siteMapCacheKey);
            return builderSet.CacheDetails;
        }

        #endregion ISiteMapCreator Members

        protected virtual ISiteMapBuilderSet GetBuilderSet(string siteMapCacheKey)
        {
            var builderSetName = siteMapCacheKeyToBuilderSetMapper.GetBuilderSetName(siteMapCacheKey);
            var builderSet = siteMapBuilderSetStrategy.GetBuilderSet(builderSetName);
            builderSet.SiteMapCacheKey = siteMapCacheKey;
            return builderSet;
        }
    }
}