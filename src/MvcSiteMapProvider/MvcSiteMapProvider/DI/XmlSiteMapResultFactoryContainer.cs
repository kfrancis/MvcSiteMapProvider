using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// A specialized dependency injection container for resolving a <see cref="T:MvcSiteMapProvider.Web.Mvc.XmlSiteMapResultFactory"/> instance.
    /// </summary>
    public class XmlSiteMapResultFactoryContainer
    {
        public XmlSiteMapResultFactoryContainer(ConfigurationSettings settings)
        {
            var siteMapLoaderContainer = new SiteMapLoaderContainer(settings);
            siteMapLoader = siteMapLoaderContainer.ResolveSiteMapLoader();
            mvcContextFactory = new MvcContextFactory();
            bindingFactory = new BindingFactory();
            bindingProvider = new BindingProvider(bindingFactory, mvcContextFactory);
            urlPath = new UrlPath(mvcContextFactory, bindingProvider);
            cultureContextFactory = new CultureContextFactory();
        }

        private readonly ISiteMapLoader siteMapLoader;
        private readonly IUrlPath urlPath;
        private readonly IMvcContextFactory mvcContextFactory;
        private readonly ICultureContextFactory cultureContextFactory;
        private readonly IBindingProvider bindingProvider;
        private readonly IBindingFactory bindingFactory;

        public IXmlSiteMapResultFactory ResolveXmlSiteMapResultFactory()
        {
            return new XmlSiteMapResultFactory(
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }
    }
}