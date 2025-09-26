using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    ///     XmlSiteMapResult class.
    /// </summary>
    public class XmlSiteMapResult
        : ActionResult
    {
        /// <summary>
        ///     Maximal number of links per sitemap file.
        /// </summary>
        /// <remarks>
        ///     This number should be 50000 in theory, see http://www.sitemaps.org/protocol.php#sitemapIndex_sitemap.
        ///     Since sitemap files can be maximal 10MB per file and calculating the total sitemap size would degrade performance,
        ///     an average cap of 35000 has been chosen.
        /// </remarks>
        private const int MaxNumberOfLinksPerFile = 35000;

        private readonly ICultureContextFactory _cultureContextFactory;
        private readonly HashSet<string> _duplicateUrlCheck = [];

        private readonly ISiteMapLoader _siteMapLoader;
        private readonly IUrlPath _urlPath;

        /// <summary>
        ///     Source metadata
        /// </summary>
        private readonly Dictionary<string, object?> _sourceMetadata =
            new() { { "HtmlHelper", typeof(XmlSiteMapResult).FullName } };

        public XmlSiteMapResult(
            int page,
            ISiteMapNode rootNode,
            IEnumerable<string> siteMapCacheKeys,
            string baseUrl,
            string siteMapUrlTemplate,
            ISiteMapLoader siteMapLoader,
            IUrlPath urlPath,
            ICultureContextFactory cultureContextFactory)
        {
            Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            Page = page;
            RootNode = rootNode;
            SiteMapCacheKeys = siteMapCacheKeys;
            BaseUrl = baseUrl;
            SiteMapUrlTemplate = siteMapUrlTemplate;
            _siteMapLoader = siteMapLoader ?? throw new ArgumentNullException(nameof(siteMapLoader));
            _urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            _cultureContextFactory =
                cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        /// <summary>
        ///     Gets or sets the XML namespace.
        /// </summary>
        /// <value>The XML namespace.</value>
        private XNamespace Ns { get; }

        /// <summary>
        ///     Gets or sets the root node.
        /// </summary>
        /// <value>The root node.</value>
        private ISiteMapNode? RootNode { get; set; }

        /// <summary>
        ///     Gets or sets the site map cache keys.
        /// </summary>
        /// <value>The site map cache keys.</value>
        private IEnumerable<string> SiteMapCacheKeys { get; }

        /// <summary>
        ///     Gets or sets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        private string BaseUrl { get; }

        /// <summary>
        ///     Gets or sets the site map URL template.
        /// </summary>
        /// <value>The site map URL template.</value>
        private string SiteMapUrlTemplate { get; }

        /// <summary>
        ///     Gets or sets the page.
        /// </summary>
        /// <value>The page.</value>
        private int Page { get; set; }


        /// <summary>
        ///     Executes the sitemap index result.
        /// </summary>
        /// <param name="context">
        ///     The context in which the result is executed. The context information includes the controller,
        ///     HTTP content, request context, and route data.
        /// </param>
        /// <param name="flattenedHierarchyCount">The flattened hierarchy count.</param>
        protected virtual void ExecuteSitemapIndexResult(ControllerContext context, long flattenedHierarchyCount)
        {
            // Count the number of pages
            var numPages = Math.Ceiling((double)flattenedHierarchyCount / MaxNumberOfLinksPerFile);

            // Output content type
            context.HttpContext.Response.ContentType = "text/xml";

            // Generate sitemap index
            var sitemapIndex = new XElement(Ns + "sitemapindex");
            sitemapIndex.Add(GenerateSiteMapIndexElements(Convert.ToInt32(numPages), BaseUrl, SiteMapUrlTemplate)
                .ToArray<object>());

            // Generate sitemap
            var xmlSiteMap = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                sitemapIndex);

            // Write XML
            using var outputStream = RetrieveOutputStream(context);
            using (var writer = XmlWriter.Create(outputStream))
            {
                xmlSiteMap.WriteTo(writer);
            }

            outputStream.Flush();
        }

        /// <summary>
        ///     Executes the sitemap result.
        /// </summary>
        /// <param name="context">
        ///     The context in which the result is executed. The context information includes the controller,
        ///     HTTP content, request context, and route data.
        /// </param>
        /// <param name="flattenedHierarchy">The flattened hierarchy.</param>
        /// <param name="flattenedHierarchyCount">The flattened hierarchy count.</param>
        /// <param name="page">The page.</param>
        protected virtual void ExecuteSitemapResult(ControllerContext context,
            IEnumerable<ISiteMapNode> flattenedHierarchy, long flattenedHierarchyCount, int page)
        {
            // Output content type
            context.HttpContext.Response.ContentType = "text/xml";

            // Generate URL set
            var urlSet = new XElement(Ns + "urlset");
            urlSet.Add(GenerateUrlElements(flattenedHierarchy.Skip((page - 1) * MaxNumberOfLinksPerFile)
                    .Take(MaxNumberOfLinksPerFile)).ToArray<object>());

            // Generate sitemap
            var xmlSiteMap = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                urlSet);

            // Write XML
            using var outputStream = RetrieveOutputStream(context);
            using (var writer = XmlWriter.Create(outputStream))
            {
                xmlSiteMap.WriteTo(writer);
            }

            outputStream.Flush();
        }

        /// <summary>
        ///     Enables processing of the result of an action method by a custom type that inherits from the
        ///     <see cref="T:System.Web.Mvc.ActionResult" /> class.
        /// </summary>
        /// <param name="context">
        ///     The context in which the result is executed. The context information includes the controller,
        ///     HTTP content, request context, and route data.
        /// </param>
        public override void ExecuteResult(ControllerContext context)
        {
            var flattenedHierarchy = new HashSet<ISiteMapNode>();

            // Flatten link hierarchy
            if (SiteMapCacheKeys.Any())
            {
                foreach (var key in SiteMapCacheKeys)
                {
                    var siteMap = _siteMapLoader.GetSiteMap(key);
                    if (siteMap == null)
                    {
                        throw new UnknownSiteMapException(Messages.UnknownSiteMap);
                    }

                    RootNode = siteMap.RootNode;
                    foreach (var item in FlattenHierarchy(RootNode, context, siteMap.VisibilityAffectsDescendants))
                    {
                        flattenedHierarchy.Add(item);
                    }
                }
            }
            else
            {
                var siteMapRootNode = RootNode;
                foreach (var item in FlattenHierarchy(RootNode, context,
                             siteMapRootNode is { SiteMap.VisibilityAffectsDescendants: true }))
                {
                    flattenedHierarchy.Add(item);
                }
            }

            var flattenedHierarchyCount = flattenedHierarchy.LongCount();

            // Determine type of sitemap to generate: sitemap index file or sitemap file
            if (flattenedHierarchyCount > MaxNumberOfLinksPerFile && Page == 0)
            {
                // Sitemap index file
                ExecuteSitemapIndexResult(context, flattenedHierarchyCount);
            }
            else if (flattenedHierarchyCount > MaxNumberOfLinksPerFile && Page > 0)
            {
                // Sitemap file for links of page X
                ExecuteSitemapResult(context, flattenedHierarchy, flattenedHierarchyCount, Page);
            }
            else
            {
                // Sitemap file for all links
                ExecuteSitemapResult(context, flattenedHierarchy, flattenedHierarchyCount, 1);
            }
        }

        /// <summary>
        ///     Generates the sitemap index elements.
        /// </summary>
        /// <param name="numPages">The number of pages.</param>
        /// <param name="baseUrl">
        ///     The base URL.
        /// </param>
        /// <param name="siteMapUrlTemplate">The site map URL template.</param>
        /// <returns>The sitemap index elements.</returns>
        protected virtual IEnumerable<XElement> GenerateSiteMapIndexElements(int numPages, string baseUrl,
            string siteMapUrlTemplate)
        {
            // Generate elements
            for (var i = 1; i <= numPages; i++)
            {
                var templateUrl = "~/" + siteMapUrlTemplate.Replace("{page}", i.ToString());
                var pageUrl = _urlPath.MakeUrlAbsolute(baseUrl, templateUrl);
                yield return new XElement(Ns + "sitemap", new XElement(Ns + "loc", pageUrl));
            }
        }

        /// <summary>
        ///     Generates the URL elements.
        /// </summary>
        /// <param name="siteMapNodes">The site map nodes.</param>
        /// <returns>The URL elements.</returns>
        protected virtual IEnumerable<XElement> GenerateUrlElements(IEnumerable<ISiteMapNode> siteMapNodes)
        {
            // Iterate all nodes
            foreach (var siteMapNode in siteMapNodes)
            {
                // Generate element
                var nodeUrl = _urlPath.MakeUrlAbsolute(BaseUrl, siteMapNode.Url);
                var urlElement = new XElement(Ns + "url",
                    new XElement(Ns + "loc", nodeUrl));

                // Switch to the invariant culture when evaluating DateTime.MinValue, doing ToLower(), and doing string.Format()
                using (_cultureContextFactory.CreateInvariant())
                {
                    // Generate element properties
                    if (siteMapNode.LastModifiedDate > DateTime.MinValue)
                    {
                        urlElement.Add(new XElement(Ns + "lastmod", siteMapNode.LastModifiedDate.ToUniversalTime()));
                    }

                    if (siteMapNode.ChangeFrequency != ChangeFrequency.Undefined)
                    {
                        urlElement.Add(
                            new XElement(Ns + "changefreq", siteMapNode.ChangeFrequency.ToString().ToLower()));
                    }

                    if (siteMapNode.UpdatePriority != UpdatePriority.Undefined)
                    {
                        urlElement.Add(new XElement(Ns + "priority",
                            $"{(double)siteMapNode.UpdatePriority / 100:0.0}"));
                    }
                }

                // Return
                yield return urlElement;
            }
        }

        /// <summary>
        ///     Generates flat list of SiteMapNode from SiteMap hierarchy.
        /// </summary>
        /// <param name="startingNode">The starting node.</param>
        /// <param name="context">The controller context.</param>
        /// <param name="visibilityAffectsDescendants">
        ///     A boolean indicating whether visibility of the current node should affect
        ///     the visibility of descendant nodes.
        /// </param>
        /// <returns>A flat list of SiteMapNode.</returns>
        protected virtual IEnumerable<ISiteMapNode> FlattenHierarchy(ISiteMapNode startingNode,
            ControllerContext context, bool visibilityAffectsDescendants)
        {
            // Inaccessible - don't process current node or any descendant nodes.
            if (!startingNode.IsAccessibleToUser() ||
                visibilityAffectsDescendants && !startingNode.IsVisible(_sourceMetadata))
            {
                yield break;
            }

            if (ShouldNodeRender(startingNode, context))
            {
                yield return startingNode;
            }

            if (!startingNode.HasChildNodes)
            {
                yield break;
            }

            // Make sure all child nodes are accessible prior to rendering them...
            foreach (var node in startingNode.ChildNodes)
            {
                foreach (var childNode in FlattenHierarchy(node, context, visibilityAffectsDescendants))
                {
                    yield return childNode;
                }
            }
        }

        /// <summary>
        ///     Checks all rules to determine if the current node should render in the sitemap.
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="context">The controller context</param>
        /// <returns><b>true</b> if the current node should be rendered; otherwise<b>false</b>.</returns>
        protected virtual bool ShouldNodeRender(ISiteMapNode node, ControllerContext context)
        {
            return node.Clickable &&
                   node.IsVisible(_sourceMetadata) &&
                   !node.HasExternalUrl(context.HttpContext) &&
                   IsCanonicalUrl(node, context.HttpContext) &&
                   !node.HasNoIndexAndNoFollow &&
                   !IsDuplicateUrl(node);
        }

        /// <summary>
        ///     Determines whether the URL of the current node is a canonical node.
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="context">
        ///    The HTTP context.
        /// </param>
        /// <returns><c>true</c> if the node's URL is canonical; otherwise<c>false</c>.</returns>
        protected virtual bool IsCanonicalUrl(ISiteMapNode node, HttpContextBase context)
        {
            var canonicalUrl = node.CanonicalUrl;
            if (string.IsNullOrEmpty(canonicalUrl))
            {
                return true;
            }

            var absoluteUrl = string.Empty;

            if (string.IsNullOrEmpty(node.Protocol) && string.IsNullOrEmpty(node.HostName))
            {
                var mvcContextFactory = new MvcContextFactory();

                // Use the HTTP protocol to force an absolute URL to compare with if no protocol was provided.
                var protocol = string.IsNullOrEmpty(node.Protocol) ? Uri.UriSchemeHttp : node.Protocol;

                // Create a URI with the home page and no query string values.
                if (context.Request.Url == null)
                {
                    return absoluteUrl.Equals(node.CanonicalUrl, StringComparison.Ordinal) || absoluteUrl.Equals("#");
                }

                var uri = new Uri(context.Request.Url, "/");

                // Create a TextWriter with null stream as a backing stream 
                // which doesn't consume resources
                using var nullWriter = new StreamWriter(Stream.Null);
                var newContext = mvcContextFactory.CreateHttpContext(node, uri, nullWriter);
                absoluteUrl = _urlPath.ResolveUrl(node.Url, protocol, node.HostName, newContext);
            }
            else
            {
                absoluteUrl = node.Url;
            }

            return absoluteUrl.Equals(node.CanonicalUrl, StringComparison.Ordinal) || absoluteUrl.Equals("#");
        }

        /// <summary>
        ///     Determines whether the URL is already included in the sitemap.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns><b>true</b> if the URL of the node is a duplicate; otherwise <b>false</b>.</returns>
        protected virtual bool IsDuplicateUrl(ISiteMapNode node)
        {
            var absoluteUrl = _urlPath.MakeUrlAbsolute(BaseUrl, node.Url);
            var isDuplicate = _duplicateUrlCheck.Contains(absoluteUrl);
            if (!isDuplicate)
            {
                _duplicateUrlCheck.Add(absoluteUrl);
            }

            return isDuplicate;
        }

        /// <summary>
        ///     Retrieves the output stream.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected virtual Stream RetrieveOutputStream(ControllerContext context)
        {
            // Output stream
            var outputStream = context.HttpContext.Response.OutputStream;

            // Check if output can be GZip compressed
            var headers = context.RequestContext.HttpContext.Request.Headers;
            if (headers["Accept-encoding"] != null && headers["Accept-encoding"].ToLowerInvariant().Contains("gzip"))
            {
                context.RequestContext.HttpContext.Response.AppendHeader("Content-encoding", "gzip");
                outputStream = new GZipStream(context.HttpContext.Response.OutputStream, CompressionMode.Compress);
            }
            else if (headers["Accept-encoding"] != null &&
                     headers["Accept-encoding"].ToLowerInvariant().Contains("deflate"))
            {
                context.RequestContext.HttpContext.Response.AppendHeader("Content-encoding", "deflate");
                outputStream = new DeflateStream(context.HttpContext.Response.OutputStream, CompressionMode.Compress);
            }

            return outputStream;
        }
    }
}
