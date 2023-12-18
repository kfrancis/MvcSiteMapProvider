using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Security;
using MvcSiteMapProvider.Web.Mvc;
using System;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Provider for plug-ins used by <see cref="T:MvcSiteMapProvider.SiteMap"/>.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapPluginProvider
        : ISiteMapPluginProvider
    {
        public SiteMapPluginProvider(
            IMvcResolver mvcResolver,
            ISiteMapBuilder siteMapBuilder,
            IAclModule aclModule
            )
        {
            this.siteMapBuilder = siteMapBuilder ?? throw new ArgumentNullException(nameof(siteMapBuilder));
            this.mvcResolver = mvcResolver ?? throw new ArgumentNullException(nameof(mvcResolver));
            this.aclModule = aclModule ?? throw new ArgumentNullException(nameof(aclModule));
        }

        protected readonly ISiteMapBuilder siteMapBuilder;
        protected readonly IMvcResolver mvcResolver;
        protected readonly IAclModule aclModule;

        #region ISiteMapPluginProvider Members

        public virtual ISiteMapBuilder SiteMapBuilder
        {
            get { return siteMapBuilder; }
        }

        public virtual IMvcResolver MvcResolver
        {
            get { return mvcResolver; }
        }

        public virtual IAclModule AclModule
        {
            get { return aclModule; }
        }

        #endregion ISiteMapPluginProvider Members
    }
}