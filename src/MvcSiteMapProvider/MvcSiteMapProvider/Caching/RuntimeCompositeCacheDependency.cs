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
        protected readonly ICacheDependency[] cacheDependencies;

        public RuntimeCompositeCacheDependency(
                    params ICacheDependency[] cacheDependencies
            )
        {
            this.cacheDependencies = cacheDependencies ?? throw new ArgumentNullException(nameof(cacheDependencies));
        }

        public object Dependency
        {
            get
            {
                if (cacheDependencies.Length > 0)
                {
                    var list = new List<ChangeMonitor>();
                    foreach (var item in cacheDependencies)
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