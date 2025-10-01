namespace MvcSiteMapProvider.Caching;

/// <summary>
///     Provides a caching mechanism for site map data, enabling efficient retrieval and storage of site map instances.
/// </summary>
/// <remarks>
///     SiteMapCache leverages the underlying cache provider to manage site map objects, reducing the need
///     for repeated site map generation and improving application performance. This class is typically used to optimize
///     access to site navigation structures in web applications.
/// </remarks>
public class SiteMapCache
    : MicroCache<ISiteMap>, ISiteMapCache
{
    /// <summary>
    ///     Initializes a new instance of the SiteMapCache class using the specified cache provider for site map data.
    /// </summary>
    /// <param name="cacheProvider">The cache provider used to store and retrieve ISiteMap instances. Cannot be null.</param>
    public SiteMapCache(
        ICacheProvider<ISiteMap> cacheProvider
    ) : base(cacheProvider)
    {
    }
}
