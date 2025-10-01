namespace MvcSiteMapProvider.Caching;

/// <summary>
///     An <see cref="T:MvcSiteMapProvider.Caching.ICacheDependency" /> implementation that can be used to indicate
///     there are no cache dependencies.
/// </summary>
public class NullCacheDependency
    : ICacheDependency
{
    public object? Dependency => null;
}
