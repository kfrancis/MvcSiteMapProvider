using System;
using MvcSiteMapProvider.Caching;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Provides a named set of services that can be used to build a <see cref="T:MvcSiteMapProvider.ISiteMap"/>.
    /// </summary>
    public class SiteMapBuilderSet
        : ISiteMapBuilderSet
    {
        private readonly ICacheDetails _cacheDetails;

        private readonly bool _enableLocalization;

        private readonly string _instanceName;

        private readonly bool _securityTrimmingEnabled;

        private readonly ISiteMapBuilder _siteMapBuilder;

        private readonly bool _useTitleIfDescriptionNotProvided;

        private readonly bool _visibilityAffectsDescendants;

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
            _instanceName = instanceName;
            _securityTrimmingEnabled = securityTrimmingEnabled;
            _enableLocalization = enableLocalization;
            _visibilityAffectsDescendants = visibilityAffectsDescendants;
            _useTitleIfDescriptionNotProvided = useTitleIfDescriptionNotProvided;
            _siteMapBuilder = siteMapBuilder ?? throw new ArgumentNullException(nameof(siteMapBuilder));
            _cacheDetails = cacheDetails ?? throw new ArgumentNullException(nameof(cacheDetails));
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
            get { return _siteMapBuilder; }
        }

        public virtual ICacheDetails CacheDetails
        {
            get { return _cacheDetails; }
        }

        public virtual bool EnableLocalization
        {
            get { return _enableLocalization; }
        }

        public virtual bool SecurityTrimmingEnabled
        {
            get { return _securityTrimmingEnabled; }
        }

        public virtual string SiteMapCacheKey { get; set; }

        public virtual bool UseTitleIfDescriptionNotProvided
        {
            get { return _useTitleIfDescriptionNotProvided; }
        }

        public virtual bool VisibilityAffectsDescendants
        {
            get { return _visibilityAffectsDescendants; }
        }

        public virtual bool AppliesTo(string builderSetName)
        {
            return _instanceName.Equals(builderSetName, StringComparison.Ordinal);
        }
    }
}
