using System;
using System.Web.Caching;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     A wrapper class to create a concrete instance of <see cref="System.Web.Caching.CacheDependency" /> without creating
///     a dependency on the System.Web library.
/// </summary>
public class AspNetFileCacheDependency
    : ICacheDependency
{
    private readonly string _fileName;

    public AspNetFileCacheDependency(
        string fileName
    )
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        _fileName = fileName;
    }

    public object? Dependency => new CacheDependency(_fileName);
}
