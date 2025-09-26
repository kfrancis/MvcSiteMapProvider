using MvcSiteMapProvider.Web.UrlResolver;
using System;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Provider for plugins used by <see cref="T:MvcSiteMapProvider.SiteMapNode"/>.
    /// </summary>
    public class SiteMapNodePluginProvider
        : ISiteMapNodePluginProvider
    {
        public SiteMapNodePluginProvider(
            IDynamicNodeProviderStrategy dynamicNodeProviderStrategy,
            ISiteMapNodeUrlResolverStrategy siteMapNodeUrlResolverStrategy,
            ISiteMapNodeVisibilityProviderStrategy siteMapNodeVisibilityProviderStrategy
            )
        {
            this.dynamicNodeProviderStrategy = dynamicNodeProviderStrategy ?? throw new ArgumentNullException(nameof(dynamicNodeProviderStrategy));
            this.siteMapNodeUrlResolverStrategy = siteMapNodeUrlResolverStrategy ?? throw new ArgumentNullException(nameof(siteMapNodeUrlResolverStrategy));
            this.siteMapNodeVisibilityProviderStrategy = siteMapNodeVisibilityProviderStrategy ?? throw new ArgumentNullException(nameof(siteMapNodeVisibilityProviderStrategy));
        }

        protected readonly IDynamicNodeProviderStrategy dynamicNodeProviderStrategy;
        protected readonly ISiteMapNodeUrlResolverStrategy siteMapNodeUrlResolverStrategy;
        protected readonly ISiteMapNodeVisibilityProviderStrategy siteMapNodeVisibilityProviderStrategy;

        #region ISiteMapNodePluginService Members

        public virtual IDynamicNodeProviderStrategy DynamicNodeProviderStrategy => this.dynamicNodeProviderStrategy;

        public virtual ISiteMapNodeUrlResolverStrategy UrlResolverStrategy => this.siteMapNodeUrlResolverStrategy;

        public virtual ISiteMapNodeVisibilityProviderStrategy VisibilityProviderStrategy => this.siteMapNodeVisibilityProviderStrategy;

        #endregion
    }
}
