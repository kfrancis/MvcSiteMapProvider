#if !NET35
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     A wrapper class to create an IList of <see cref="System.Runtime.Caching.HostFileChangeMonitor" /> without creating
///     a dependency on the System.Runtime.Caching library.
/// </summary>
public class RuntimeFileCacheDependency
    : ICacheDependency
{
    private readonly string _fileName;

    public RuntimeFileCacheDependency(
        string fileName
    )
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        _fileName = fileName;
    }

    public virtual object? Dependency
    {
        get
        {
            var list = new List<ChangeMonitor>();
            list.Add(new HostFileChangeMonitor([_fileName]));
            return list;
        }
    }
}
#endif
