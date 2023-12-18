using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Loader;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Web.Mvc.XmlSiteMapResult"/>
    /// at runtime.
    /// </summary>
    public class XmlSiteMapResultFactory
        : IXmlSiteMapResultFactory
    {
        public XmlSiteMapResultFactory(
            ISiteMapLoader siteMapLoader,
            IUrlPath urlPath,
            ICultureContextFactory cultureContextFactory
            )
        {
            this.siteMapLoader = siteMapLoader ?? throw new ArgumentNullException(nameof(siteMapLoader));
            this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        protected readonly ISiteMapLoader siteMapLoader;
        protected readonly IUrlPath urlPath;
        protected readonly ICultureContextFactory cultureContextFactory;

        #region IXmlSiteMapResultFactory Members

        public virtual ActionResult Create(int page)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, IEnumerable<string> siteMapCacheKeys, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                DefaultRootNode,
                siteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        public virtual ActionResult Create(int page, ISiteMapNode rootNode, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                page,
                rootNode,
                DefaultSiteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        [Obsolete("Overload is invalid for sitemaps with over 35,000 links. Use Create(int page) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create()
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        [Obsolete("Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(IEnumerable<string> siteMapCacheKeys)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                siteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        [Obsolete("Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys, string baseUrl, string siteMapUrlTemplate) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(IEnumerable<string> siteMapCacheKeys, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                DefaultRootNode,
                siteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        [Obsolete("Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, ISiteMapNode rootNode) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(ISiteMapNode rootNode)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                rootNode,
                DefaultSiteMapCacheKeys,
                DefaultBaseUrl,
                DefaultSiteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        [Obsolete("Overload is invalid for sitemaps with over 35,000 links. Use Create(int page, IEnumerable<string> siteMapCacheKeys) instead. This overload will be removed in version 5.")]
        public virtual ActionResult Create(ISiteMapNode rootNode, string baseUrl, string siteMapUrlTemplate)
        {
            return new XmlSiteMapResult(
                DefaultPage,
                rootNode,
                DefaultSiteMapCacheKeys,
                baseUrl,
                siteMapUrlTemplate,
                siteMapLoader,
                urlPath,
                cultureContextFactory);
        }

        #endregion IXmlSiteMapResultFactory Members

        protected virtual int DefaultPage
        {
            get { return 0; }
        }

        protected virtual ISiteMapNode DefaultRootNode
        {
            get { return siteMapLoader.GetSiteMap().RootNode; }
        }

        protected virtual string DefaultSiteMapUrlTemplate
        {
            get { return "sitemap-{page}.xml"; }
        }

        protected virtual string DefaultBaseUrl
        {
            get { return urlPath.ResolveUrl("/", Uri.UriSchemeHttp); }
        }

        protected virtual IEnumerable<string> DefaultSiteMapCacheKeys
        {
            get { return new List<string>(); }
        }
    }
}