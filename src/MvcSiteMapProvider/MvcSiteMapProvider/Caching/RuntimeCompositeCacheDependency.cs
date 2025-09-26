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
        public RuntimeCompositeCacheDependency(
            params ICacheDependency[] cacheDependencies
            )
        {
            this.cacheDependencies = cacheDependencies ?? throw new ArgumentNullException(nameof(cacheDependencies));
        }

        protected readonly ICacheDependency[] cacheDependencies;

        public object? Dependency
        {
            get
            {
                if (!this.cacheDependencies.Any())
                {
                    return null;
                }

                var list = new List<ChangeMonitor>();
                foreach (var item in this.cacheDependencies)
                {
                    if (item.Dependency is not IList<ChangeMonitor> changeMonitorList)
                    {
                        continue;
                    }

                    foreach (var changeMonitor in changeMonitorList)
                    {
                        list.Add(changeMonitor);
                    }
                }
                return list;
            }
        }

    }
}
#endif