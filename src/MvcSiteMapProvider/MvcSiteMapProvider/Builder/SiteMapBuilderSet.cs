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
        protected readonly ICacheDetails cacheDetails;

        protected readonly bool enableLocalization;

        protected readonly string instanceName;

        protected readonly bool securityTrimmingEnabled;

        protected readonly ISiteMapBuilder siteMapBuilder;

        protected readonly bool useTitleIfDescriptionNotProvided;

        protected readonly bool visibilityAffectsDescendants;

        public SiteMapBuilderSet(
                                                                   string instanceName,
           bool securityTrimmingEnabled,
           bool enableLocalization,
           bool visibilityAffectsDescendants,
           bool useTitleIfDescriptionNotProvided,
           ISiteMapBuilder siteMapBuilder,
           ICacheDetails cacheDetails
           )
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new ArgumentNullException(nameof(instanceName));
            this.instanceName = instanceName;
            this.securityTrimmingEnabled = securityTrimmingEnabled;
            this.enableLocalization = enableLocalization;
            this.visibilityAffectsDescendants = visibilityAffectsDescendants;
            this.useTitleIfDescriptionNotProvided = useTitleIfDescriptionNotProvided;
            this.siteMapBuilder = siteMapBuilder ?? throw new ArgumentNullException(nameof(siteMapBuilder));
            this.cacheDetails = cacheDetails ?? throw new ArgumentNullException(nameof(cacheDetails));
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

        public virtual ISiteMapBuilder Builder
        {
            get { return siteMapBuilder; }
        }

        public virtual ICacheDetails CacheDetails
        {
            get { return cacheDetails; }
        }

        public virtual bool EnableLocalization
        {
            get { return enableLocalization; }
        }

        public virtual bool SecurityTrimmingEnabled
        {
            get { return securityTrimmingEnabled; }
        }

        public virtual string SiteMapCacheKey { get; set; }

        public virtual bool UseTitleIfDescriptionNotProvided
        {
            get { return useTitleIfDescriptionNotProvided; }
        }

        public virtual bool VisibilityAffectsDescendants
        {
            get { return visibilityAffectsDescendants; }
        }

        public virtual bool AppliesTo(string builderSetName)
        {
            return instanceName.Equals(builderSetName, StringComparison.Ordinal);
        }
    }
}