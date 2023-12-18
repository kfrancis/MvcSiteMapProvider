using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using System;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.RequestCacheableSiteMap"/>
    /// at runtime.
    /// </summary>
    public class SiteMapFactory
        : ISiteMapFactory
    {
        protected readonly IActionMethodParameterResolverFactory actionMethodParameterResolverFactory;

        protected readonly IControllerTypeResolverFactory controllerTypeResolverFactory;

        protected readonly IMvcContextFactory mvcContextFactory;

        protected readonly IMvcResolverFactory mvcResolverFactory;

        protected readonly ISiteMapPluginProviderFactory pluginProviderFactory;

        protected readonly ISiteMapChildStateFactory siteMapChildStateFactory;

        protected readonly IUrlPath urlPath;

        public SiteMapFactory(
                                                                    ISiteMapPluginProviderFactory pluginProviderFactory,
            IMvcResolverFactory mvcResolverFactory,
            IMvcContextFactory mvcContextFactory,
            ISiteMapChildStateFactory siteMapChildStateFactory,
            IUrlPath urlPath,
            IControllerTypeResolverFactory controllerTypeResolverFactory,
            IActionMethodParameterResolverFactory actionMethodParameterResolverFactory
            )
        {
            this.pluginProviderFactory = pluginProviderFactory ?? throw new ArgumentNullException(nameof(pluginProviderFactory));
            this.mvcResolverFactory = mvcResolverFactory ?? throw new ArgumentNullException(nameof(mvcResolverFactory));
            this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
            this.siteMapChildStateFactory = siteMapChildStateFactory ?? throw new ArgumentNullException(nameof(siteMapChildStateFactory));
            this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            this.controllerTypeResolverFactory = controllerTypeResolverFactory ?? throw new ArgumentNullException(nameof(controllerTypeResolverFactory));
            this.actionMethodParameterResolverFactory = actionMethodParameterResolverFactory ?? throw new ArgumentNullException(nameof(actionMethodParameterResolverFactory));
        }

        public virtual ISiteMap Create(ISiteMapBuilder siteMapBuilder, ISiteMapSettings siteMapSettings)
        {
            var routes = mvcContextFactory.GetRoutes();
            var requestCache = mvcContextFactory.GetRequestCache();

            // IMPORTANT: We need to ensure there is one instance of controllerTypeResolver and
            // one instance of ActionMethodParameterResolver per SiteMap instance because each of
            // these classes does internal caching.
            var controllerTypeResolver = controllerTypeResolverFactory.Create(routes);
            var actionMethodParameterResolver = actionMethodParameterResolverFactory.Create();
            var mvcResolver = mvcResolverFactory.Create(controllerTypeResolver, actionMethodParameterResolver);
            var pluginProvider = pluginProviderFactory.Create(siteMapBuilder, mvcResolver);

            return new RequestCacheableSiteMap(
                pluginProvider,
                mvcContextFactory,
                siteMapChildStateFactory,
                urlPath,
                siteMapSettings,
                requestCache);
        }
    }
}