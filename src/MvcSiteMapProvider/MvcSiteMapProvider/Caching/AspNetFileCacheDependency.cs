using System;
using System.Web.Caching;

namespace MvcSiteMapProvider.Caching
{
    /// <summary>
    /// A wrapper class to create a concrete instance of <see cref="System.Web.Caching.CacheDependency"/> without creating
    /// a dependency on the System.Web library.
    /// </summary>
    public class AspNetFileCacheDependency
        : ICacheDependency
    {
        protected readonly string fileName;

        public AspNetFileCacheDependency(
                    string fileName
            )
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            this.fileName = fileName;
        }

        public object Dependency
        {
            get { return new CacheDependency(fileName); }
        }
    }
}