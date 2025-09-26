using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Loader;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    ///     An abstract factory that can be used to create new instances of
    ///     <see cref="T:MvcSiteMapProvider.Web.Mvc.XmlSiteMapResult" />
    ///     at runtime.
    /// </summary>
    public class XmlSiteMapResultFactory
        : IXmlSiteMapResultFactory
    {
        private readonly ICultureContextFactory _cultureContextFactory;

        private readonly ISiteMapLoader _siteMapLoader;
        private readonly IUrlPath _urlPath;

        public XmlSiteMapResultFactory(
            ISiteMapLoader siteMapLoader,
            IUrlPath urlPath,
            ICultureContextFactory cultureContextFactory
        )
        {
            _siteMapLoader = siteMapLoader ?? throw new ArgumentNullException(nameof(siteMapLoader));
            _urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            _cultureContextFactory =
                cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        protected virtual int DefaultPage => 0;

        protected virtual ISiteMapNode? DefaultRootNode => _siteMapLoader.GetSiteMap()?.RootNode;

        protected virtual string DefaultSiteMapUrlTemplate => "sitemap-{page}.xml";

        protected virtual string DefaultBaseUrl => _urlPath.ResolveUrl("/", Uri.UriSchemeHttp);

        protected virtual IEnumerable<string> DefaultSiteMapCacheKeys => new List<string>();

        public virtual ActionResult Create(int page)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys, string baseUrl,
            string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        [Obsolete(
            "Overload is invalid for sitemaps with over 35,000 links. Use Create(int page) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create()
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        [Obsolete(
            "Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(IEnumerable<string> siteMapCacheKeys)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        [Obsolete(
            "Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys, string baseUrl, string siteMapUrlTemplate) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(IEnumerable<string> siteMapCacheKeys, string baseUrl,
            string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                siteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        [Obsolete(
            "Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, ISiteMapNode rootNode) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(ISiteMapNode rootNode)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }

        [Obsolete(
            "Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(ISiteMapNode rootNode, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                rootNode,
                DefaultSiteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                _siteMapLoader,
                _urlPath,
                _cultureContextFactory);
        }
    }
}
