using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Matching;
using MvcSiteMapProvider.Security;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Compilation;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Mvc.Filters;
using System.Web.Mvc;

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// A specialized dependency injection container for resolving a <see cref="T:MvcSiteMapProvider.SiteMapFactory"/> instance.
    /// </summary>
    public class SiteMapFactoryContainer
    {
        public SiteMapFactoryContainer(
            ConfigurationSettings settings,
            IMvcContextFactory mvcContextFactory,
            IUrlPath urlPath)
        {
            this.settings = settings;
            this.mvcContextFactory = mvcContextFactory;
            requestCache = this.mvcContextFactory.GetRequestCache();
            this.urlPath = urlPath;
            urlKeyFactory = new UrlKeyFactory(this.urlPath);
        }

        private readonly ConfigurationSettings settings;
        private readonly IMvcContextFactory mvcContextFactory;
        private readonly IRequestCache requestCache;
        private readonly IUrlPath urlPath;
        private readonly IUrlKeyFactory urlKeyFactory;

        public ISiteMapFactory ResolveSiteMapFactory()
        {
            return new SiteMapFactory(
                ResolveSiteMapPluginProviderFactory(),
                new MvcResolverFactory(),
                mvcContextFactory,
                ResolveSiteMapChildStateFactory(),
                urlPath,
                ResolveControllerTypeResolverFactory(),
                new ActionMethodParameterResolverFactory(new ControllerDescriptorFactory())
                );
        }

        private ISiteMapPluginProviderFactory ResolveSiteMapPluginProviderFactory()
        {
            return new SiteMapPluginProviderFactory(
                ResolveAclModule()
                );
        }

        private IAclModule ResolveAclModule()
        {
            return new CompositeAclModule(
                new AuthorizeAttributeAclModule(
                    mvcContextFactory,
                    new ControllerDescriptorFactory(),
                    new ControllerBuilderAdapter(ControllerBuilder.Current),
                    new GlobalFilterProvider()
                ),
                new XmlRolesAclModule(
                    mvcContextFactory
                    )
                );
        }

        private ISiteMapChildStateFactory ResolveSiteMapChildStateFactory()
        {
            return new SiteMapChildStateFactory(
                new GenericDictionaryFactory(),
                new SiteMapNodeCollectionFactory(),
                urlKeyFactory
                );
        }

        private IControllerTypeResolverFactory ResolveControllerTypeResolverFactory()
        {
            return new ControllerTypeResolverFactory(
                settings.ControllerTypeResolverAreaNamespacesToIgnore,
                new ControllerBuilderAdapter(ControllerBuilder.Current),
                new BuildManagerAdapter()
                );
        }
    }
}