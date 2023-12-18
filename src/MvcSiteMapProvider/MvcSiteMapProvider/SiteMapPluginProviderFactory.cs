using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Security;
using MvcSiteMapProvider.Web.Mvc;
using System;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.SiteMapPluginProvider"/>
    /// at runtime.
    /// </summary>
    public class SiteMapPluginProviderFactory
        : ISiteMapPluginProviderFactory
    {
        public SiteMapPluginProviderFactory(
            IAclModule aclModule
            )
        {
            this.aclModule = aclModule ?? throw new ArgumentNullException(nameof(aclModule));
        }

        protected readonly IAclModule aclModule;

        #region ISiteMapPluginProviderFactory Members

        public virtual ISiteMapPluginProvider Create(ISiteMapBuilder siteMapBuilder, IMvcResolver mvcResolver)
        {
            return new SiteMapPluginProvider(mvcResolver, siteMapBuilder, aclModule);
        }

        #endregion ISiteMapPluginProviderFactory Members
    }
}