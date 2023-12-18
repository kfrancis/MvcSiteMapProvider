using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.DI;
using System;
using System.Linq;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Tracks all of the registered instances of <see cref="T:MvcSiteMapProvider.Builder.ISiteMapBuilderSet"/> and
    /// allows the caller to get a specific named instance of this interface at runtime.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapBuilderSetStrategy
        : ISiteMapBuilderSetStrategy
    {
        protected readonly ISiteMapBuilderSet[] siteMapBuilderSets;

        public SiteMapBuilderSetStrategy(
                    ISiteMapBuilderSet[] siteMapBuilderSets
            )
        {
            this.siteMapBuilderSets = siteMapBuilderSets ?? throw new ArgumentNullException(nameof(siteMapBuilderSets));
        }

        public virtual ISiteMapBuilder GetBuilder(string builderSetName)
        {
            var builderSet = GetBuilderSet(builderSetName);
            return builderSet.Builder;
        }

        public virtual ISiteMapBuilderSet GetBuilderSet(string builderSetName)
        {
            var builderSet = siteMapBuilderSets.FirstOrDefault(x => x.AppliesTo(builderSetName)) ?? throw new MvcSiteMapException(string.Format(Resources.Messages.NamedBuilderSetNotFound, builderSetName));
            return builderSet;
        }

        public virtual ICacheDetails GetCacheDetails(string builderSetName)
        {
            var builderSet = GetBuilderSet(builderSetName);
            return builderSet.CacheDetails;
        }
    }
}