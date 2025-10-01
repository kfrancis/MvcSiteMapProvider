using System;
using System.Linq;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Tracks all the registered instances of <see cref="T:MvcSiteMapProvider.Builder.ISiteMapBuilderSet" /> and
///     allows the caller to get a specific named instance of this interface at runtime.
/// </summary>
[ExcludeFromAutoRegistration]
public class SiteMapBuilderSetStrategy
    : ISiteMapBuilderSetStrategy
{
    private readonly ISiteMapBuilderSet[] _siteMapBuilderSets;

    public SiteMapBuilderSetStrategy(
        ISiteMapBuilderSet[] siteMapBuilderSets
    )
    {
        _siteMapBuilderSets = siteMapBuilderSets ?? throw new ArgumentNullException(nameof(siteMapBuilderSets));
    }

    public virtual ISiteMapBuilderSet GetBuilderSet(string builderSetName)
    {
        var builderSet = _siteMapBuilderSets.FirstOrDefault(x => x.AppliesTo(builderSetName));
        return builderSet ??
               throw new MvcSiteMapException(string.Format(Messages.NamedBuilderSetNotFound, builderSetName));
    }

    public virtual ISiteMapBuilder GetBuilder(string builderSetName)
    {
        var builderSet = GetBuilderSet(builderSetName);
        return builderSet.Builder;
    }

    public virtual ICacheDetails GetCacheDetails(string builderSetName)
    {
        var builderSet = GetBuilderSet(builderSetName);
        return builderSet.CacheDetails;
    }
}
