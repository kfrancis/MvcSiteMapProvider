using System;
using MvcSiteMapProvider.Caching;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Provides a named set of services that can be used to build a <see cref="T:MvcSiteMapProvider.ISiteMap" />.
/// </summary>
public class SiteMapBuilderSet
    : ISiteMapBuilderSet
{
    private readonly string _instanceName;

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
        {
            throw new ArgumentNullException(nameof(instanceName));
        }

        _instanceName = instanceName!;
        SecurityTrimmingEnabled = securityTrimmingEnabled;
        EnableLocalization = enableLocalization;
        VisibilityAffectsDescendants = visibilityAffectsDescendants;
        UseTitleIfDescriptionNotProvided = useTitleIfDescriptionNotProvided;
        Builder = siteMapBuilder ?? throw new ArgumentNullException(nameof(siteMapBuilder));
        CacheDetails = cacheDetails ?? throw new ArgumentNullException(nameof(cacheDetails));
    }

    /// <summary>
    ///     ctor for backward compatibility,
    ///     visibilityAffectsDescendants parameter defaults to true
    ///     useTitleIfDescriptionNotProvided parameter defaults to true
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

    public virtual ISiteMapBuilder Builder { get; }

    public virtual ICacheDetails CacheDetails { get; }

    public virtual string? SiteMapCacheKey { get; set; }

    public virtual bool SecurityTrimmingEnabled { get; }

    public virtual bool EnableLocalization { get; }

    public virtual bool VisibilityAffectsDescendants { get; }

    public virtual bool UseTitleIfDescriptionNotProvided { get; }

    public virtual bool AppliesTo(string builderSetName)
    {
        return _instanceName.Equals(builderSetName, StringComparison.Ordinal);
    }
}
