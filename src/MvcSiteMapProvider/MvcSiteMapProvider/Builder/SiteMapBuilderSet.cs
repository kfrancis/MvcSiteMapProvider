using MvcSiteMapProvider.Caching;
using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Provides a named set of services that can be used to build a <see cref="T:MvcSiteMapProvider.ISiteMap"/>.
    /// </summary>
    public class SiteMapBuilderSet
        : ISiteMapBuilderSet
    {
        public SiteMapBuilderSet(
           string? instanceName,
           bool securityTrimmingEnabled,
           bool enableLocalization,
           bool visibilityAffectsDescendants,
           bool useTitleIfDescriptionNotProvided,
           ISiteMapBuilder? siteMapBuilder,
           ICacheDetails? cacheDetails
           )
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new ArgumentNullException(nameof(instanceName));

            this.instanceName = instanceName!;
            this.SecurityTrimmingEnabled = securityTrimmingEnabled;
            this.EnableLocalization = enableLocalization;
            this.VisibilityAffectsDescendants = visibilityAffectsDescendants;
            this.UseTitleIfDescriptionNotProvided = useTitleIfDescriptionNotProvided;
            this.Builder = siteMapBuilder ?? throw new ArgumentNullException(nameof(siteMapBuilder));
            this.CacheDetails = cacheDetails ?? throw new ArgumentNullException(nameof(cacheDetails));
        }

        /// <summary>
        /// ctor for backward compatibility, 
        /// visibilityAffectsDescendants parameter defaults to true
        /// useTitleIfDescriptionNotProvided parameter defaults to true
        /// </summary>
        [Obsolete("Use the overload ctor(string, bool, bool, bool, bool, ISiteMapBuilder, ICacheDetails) instead.")]
        public SiteMapBuilderSet(
            string instanceName,
            bool securityTrimmingEnabled,
            bool enableLocalization,
            ISiteMapBuilder siteMapBuilder,
            ICacheDetails cacheDetails
            ) 
            : this(
                instanceName,
                securityTrimmingEnabled,
                enableLocalization,
                true,
                true,
                siteMapBuilder,
                cacheDetails
            ) 
        { 
        }

        private readonly string instanceName;

        public virtual ISiteMapBuilder Builder { get; }

        public virtual ICacheDetails CacheDetails { get; }

        public virtual string? SiteMapCacheKey { get; set; }

        public virtual bool SecurityTrimmingEnabled { get; }

        public virtual bool EnableLocalization { get; }

        public virtual bool VisibilityAffectsDescendants { get; }

        public virtual bool UseTitleIfDescriptionNotProvided { get; }

        public virtual bool AppliesTo(string builderSetName)
        {
            return this.instanceName.Equals(builderSetName, StringComparison.Ordinal);
        }

    }
}
