using System;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     Container for passing caching instructions around as a group.
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
        if (absoluteCacheExpiration == TimeSpan.Zero)
        {
            throw new ArgumentNullException(nameof(absoluteCacheExpiration));
        }

        if (slidingCacheExpiration == TimeSpan.Zero)
        {
            throw new ArgumentNullException(nameof(slidingCacheExpiration));
        }

        AbsoluteCacheExpiration = absoluteCacheExpiration;
        SlidingCacheExpiration = slidingCacheExpiration;
        CacheDependency = cacheDependency ?? throw new ArgumentNullException(nameof(cacheDependency));
    }

    public TimeSpan AbsoluteCacheExpiration { get; }

    public TimeSpan SlidingCacheExpiration { get; }

    public ICacheDependency CacheDependency { get; }
}
