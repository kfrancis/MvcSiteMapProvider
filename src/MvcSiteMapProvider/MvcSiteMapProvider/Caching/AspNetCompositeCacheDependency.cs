using System;
using System.Linq;
using System.Web.Caching;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     A wrapper class to create a concrete instance of <see cref="System.Web.Caching.AggregateCacheDependency" /> without
///     creating
///     a dependency on the System.Web library.
/// </summary>
public class AspNetCompositeCacheDependency
    : ICacheDependency
{
    private readonly ICacheDependency[] _cacheDependencies;

    public AspNetCompositeCacheDependency(
        params ICacheDependency[] cacheDependencies
    )
    {
        _cacheDependencies = cacheDependencies ?? throw new ArgumentNullException(nameof(cacheDependencies));
    }

    public object? Dependency
    {
        get
        {
            if (!_cacheDependencies.Any())
            {
                return null;
            }

            var list = new AggregateCacheDependency();
            foreach (var item in _cacheDependencies)
            {
                list.Add((CacheDependency)item.Dependency!);
            }

            return list;
        }
    }
}
