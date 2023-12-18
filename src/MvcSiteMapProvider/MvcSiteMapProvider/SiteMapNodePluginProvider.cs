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
        protected readonly IDynamicNodeProviderStrategy dynamicNodeProviderStrategy;

        protected readonly ISiteMapNodeUrlResolverStrategy siteMapNodeUrlResolverStrategy;

        protected readonly ISiteMapNodeVisibilityProviderStrategy siteMapNodeVisibilityProviderStrategy;

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

        public virtual IDynamicNodeProviderStrategy DynamicNodeProviderStrategy
        {
            get { return dynamicNodeProviderStrategy; }
        }

        public virtual ISiteMapNodeUrlResolverStrategy UrlResolverStrategy
        {
            get { return siteMapNodeUrlResolverStrategy; }
        }

        public virtual ISiteMapNodeVisibilityProviderStrategy VisibilityProviderStrategy
        {
            get { return siteMapNodeVisibilityProviderStrategy; }
        }
    }
}