using System;
using System.Linq;
using System.Web.Caching;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A wrapper class to create a concrete instance of <see cref="System.Web.Caching.AggregateCacheDependency"/> without creating
    /// a dependency on the System.Web library.
    /// </summary>
    public class AspNetCompositeCacheDependency
        : ICacheDependency
    {
        public AspNetCompositeCacheDependency(
            params ICacheDependency[] cacheDependencies
            )
        {
            this.cacheDependencies = cacheDependencies ?? throw new ArgumentNullException(nameof(cacheDependencies));
        }

        protected readonly ICacheDependency[] cacheDependencies;

        #region ICacheDependency Members

        public object Dependency
        {
            get
            {
                if (cacheDependencies.Count() > 0)
                {
                    var list = new AggregateCacheDependency();
                    foreach (var item in cacheDependencies)
                    {
                        list.Add((CacheDependency)item.Dependency);
                    }
                    return list;
                }
                return null;
            }
        }

        #endregion ICacheDependency Members
    }
}