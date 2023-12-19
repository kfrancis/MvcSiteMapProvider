#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A wrapper class to create an IList of <see cref="System.Runtime.Caching.ChangeMonitor"/> without creating
    /// a dependency on the System.Runtime.Caching library.
    /// </summary>
    public class RuntimeCompositeCacheDependency
        : ICacheDependency
    {
        protected readonly ICacheDependency[] CacheDependencies;

        public RuntimeCompositeCacheDependency(
                    params ICacheDependency[] cacheDependencies
            )
        {
            CacheDependencies = cacheDependencies ?? throw new ArgumentNullException(nameof(cacheDependencies));
        }

        public object Dependency
        {
            get
            {
                if (CacheDependencies.Length > 0)
                {
                    var list = new List<ChangeMonitor>();
                    foreach (var item in CacheDependencies)
                    {
                        var changeMonitorList = (IList<ChangeMonitor>)item.Dependency;
                        if (changeMonitorList != null)
                        {
                            list.AddRange(changeMonitorList);
                        }
                    }
                    return list;
                }
                return null;
            }
        }
    }
}
#endif
