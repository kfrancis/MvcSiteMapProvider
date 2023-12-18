using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Matching;
using MvcSiteMapProvider.Text;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// This class acts as the root of a SiteMap object graph and maintains a map
    /// between the child <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> nodes.
    /// </summary>
    /// <remarks>
    /// This class was created by extracting the public interfaces of SiteMapProvider,
    /// StaticSiteMapProvider, and MvcSiteMapProvider.DefaultSiteMapProvider.
    /// </remarks>
    [ExcludeFromAutoRegistration]
    public class SiteMap
        : ISiteMap
    {
        // Child collections
        protected readonly IDictionary<ISiteMapNode, ISiteMapNodeCollection> childNodeCollectionTable;

        protected readonly IDictionary<string, ISiteMapNode> keyTable;

        protected readonly IMvcContextFactory mvcContextFactory;

        protected readonly IDictionary<ISiteMapNode, ISiteMapNode> parentNodeTable;

        // Services
        protected readonly ISiteMapPluginProvider pluginProvider;

        // TODO: In version 5, we should refactor this into separate services that each manage a single dictionary
        // and hide those services behind a facade service so there isn't so many responsibilities in this class.
        // This will help the process of eliminating child state factory and plugin provider which only serve to
        // reduce the number of dependencies in this class, but technically are providing unrelated services.
        protected readonly ISiteMapChildStateFactory siteMapChildStateFactory;

        // Object state
        protected readonly object synclock = new object();

        protected readonly IUrlPath urlPath;

        protected readonly IDictionary<IUrlKey, ISiteMapNode> urlTable;

        protected ISiteMapNode root;

        private readonly ISiteMapSettings siteMapSettings;

        public SiteMap(
                                                                                                    ISiteMapPluginProvider pluginProvider,
            IMvcContextFactory mvcContextFactory,
            ISiteMapChildStateFactory siteMapChildStateFactory,
            IUrlPath urlPath,
            ISiteMapSettings siteMapSettings
            )
        {
            this.pluginProvider = pluginProvider ?? throw new ArgumentNullException(nameof(pluginProvider));
            this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
            this.siteMapChildStateFactory = siteMapChildStateFactory ?? throw new ArgumentNullException(nameof(siteMapChildStateFactory));
            this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));
            this.siteMapSettings = siteMapSettings ?? throw new ArgumentNullException(nameof(siteMapSettings));

            // Initialize dictionaries
            childNodeCollectionTable = siteMapChildStateFactory.CreateChildNodeCollectionDictionary();
            keyTable = siteMapChildStateFactory.CreateKeyDictionary();
            parentNodeTable = siteMapChildStateFactory.CreateParentNodeDictionary();
            urlTable = siteMapChildStateFactory.CreateUrlDictionary();
        }

        /// <summary>
        /// Gets a string representing the cache key of the current SiteMap object. This key (which can be though of as the name) can be used
        /// to retrieve the SiteMap object. It is also used to build request-cache keys so values can persist in the same request across SiteMap builds.
        /// </summary>
        public virtual string CacheKey
        {
            get { return siteMapSettings.SiteMapCacheKey; }
        }

        /// <summary>
        /// Gets the <see cref="T:MvcSiteMapProvider.SiteMapNode"/> object that represents the currently requested page.
        /// </summary>
        /// <returns>A <see cref="T:MvcSiteMapProvider.SiteMapNode"/> that represents the currently requested page; otherwise,
        /// null, if the <see cref="T:MvcSiteMapProvider.SiteMapNode"/> is not found or cannot be returned for the current user.</returns>
        public virtual ISiteMapNode CurrentNode
        {
            get
            {
                var currentNode = FindSiteMapNodeFromCurrentContext();
                return ReturnNodeIfAccessible(currentNode);
            }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether localized values of <see cref="T:MvcSiteMapProvider.SiteMapNode">SiteMapNode</see>
        /// attributes are returned.
        /// </summary>
        /// <remarks>
        /// The EnableLocalization property is used for the get accessor of the Title and Description properties, as well as additional
        /// Attributes properties of a SiteMapNode object.
        /// </remarks>
        public virtual bool EnableLocalization
        {
            get { return siteMapSettings.EnableLocalization; }
        }

        /// <summary>
        /// Gets whether the current sitemap is read-only.
        /// </summary>
        /// <value><c>true</c> if the current sitemap is read-only; otherwise <c>false</c>.</value>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Get or sets the resource key that is used for localizing <see cref="T:MvcSiteMapProvider.SiteMapNode"/> attributes.
        /// </summary>
        /// <remarks>
        /// The ResourceKey property is used with the GetImplicitResourceString method of the <see cref="T:MvcSiteMapProvider.SiteMapNode"/> class.
        /// For the Title and Description properties, as well as any additional attributes that are defined in the Attributes collection of the
        /// <see cref="T:MvcSiteMapProvider.SiteMapNode"/> object, the GetImplicitResourceString method takes precedence over the
        /// GetExplicitResourceString when the localization is enabled with the EnableLocalization property set to true.
        /// </remarks>
        public virtual string ResourceKey
        {
            get { return siteMapSettings.SiteMapCacheKey; }
            set { /* do nothing */ }
        }

        /// <summary>
        /// Gets the root <see cref="T:MvcSiteMapProvider.SiteMapNode"/> object of the site map data that the current provider represents.
        /// </summary>
        public virtual ISiteMapNode RootNode
        {
            get { return ReturnNodeIfAccessible(root); }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether a site map provider filters site map nodes based on a user's role.
        /// </summary>
        public bool SecurityTrimmingEnabled
        {
            get { return siteMapSettings.SecurityTrimmingEnabled; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether the site map nodes should use the value of the Title
        /// property for the default if no value for Description is provided.
        /// </summary>
        public bool UseTitleIfDescriptionNotProvided
        {
            get { return siteMapSettings.UseTitleIfDescriptionNotProvided; }
        }

        /// <summary>
        /// Gets a Boolean value indicating whether the visibility property of the current node
        /// will affect the descendant nodes.
        /// </summary>
        public bool VisibilityAffectsDescendants
        {
            get { return siteMapSettings.VisibilityAffectsDescendants; }
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        protected virtual HttpContextBase HttpContext
        { get { return mvcContextFactory.CreateHttpContext(); } }

        /// <summary>
        /// Adds a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> object to the node collection that is maintained by the site map provider.
        /// </summary>
        /// <param name="node">The <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> to add to the node collection maintained by the provider.</param>
        public virtual void AddNode(ISiteMapNode node)
        {
            AddNode(node, null);
        }

        /// <summary>
        /// Adds a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> to the collections that are maintained by the site map provider and establishes a
        /// parent/child relationship between the <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> objects.
        /// </summary>
        /// <param name="node">The <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> to add to the site map provider.</param>
        /// <param name="parentNode">The <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> under which to add <paramref name="node"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="node"/> is null.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <see cref="P:MvcSiteMapProvider.SiteMapNode.Url"/> or <see cref="P:MvcSiteMapProvider.SiteMapNode.Key"/> is already registered with
        /// the <see cref="T:MvcSiteMapProvider.SiteMap"/>. A site map node must be made up of pages with unique URLs or keys.
        /// </exception>
        public virtual void AddNode(ISiteMapNode node, ISiteMapNode parentNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            AssertSiteMapNodeConfigurationIsValid(node);

            // Add the node
            AddNodeInternal(node, parentNode);
        }

        public virtual void BuildSiteMap()
        {
            // If this was called before, just ignore this call.
            if (root != null) return;
            root = pluginProvider.SiteMapBuilder.BuildSiteMap(this, root);
            if (root == null)
            {
                throw new MvcSiteMapException(Resources.Messages.SiteMapRootNodeNotDefined);
            }
        }

        public virtual void Clear()
        {
            lock (synclock)
            {
                root = null;
                childNodeCollectionTable.Clear();
                urlTable.Clear();
                parentNodeTable.Clear();
                keyTable.Clear();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> object that represents the page at the specified URL.
        /// </summary>
        /// <param name="rawUrl">A URL that identifies the page for which to retrieve a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/>.</param>
        /// <returns>A <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> that represents the page identified by rawURL; otherwise, <b>null</b>,
        /// if no corresponding <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> is found or if security trimming is enabled and the
        /// <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> cannot be returned for the current user.</returns>
        public virtual ISiteMapNode FindSiteMapNode(string rawUrl)
        {
            if (rawUrl == null)
            {
                throw new ArgumentNullException(nameof(rawUrl));
            }
            rawUrl = rawUrl.Trim();
            if (rawUrl.Length == 0)
            {
                return null;
            }

            // NOTE: If the URL passed is absolute, the public facing URL will be ignored
            // and the current URL will be the absolute URL that is passed.
            var publicFacingUrl = urlPath.GetPublicFacingUrl(HttpContext);
            var currentUrl = new Uri(publicFacingUrl, rawUrl);

            // Search the internal dictionary for the URL that is registered manually.
            var node = FindSiteMapNodeFromUrl(currentUrl.PathAndQuery, currentUrl.AbsolutePath, currentUrl.Host, HttpContext.CurrentHandler);

            // Search for the URL by creating a context based on the new URL and matching route values.
            if (node == null)
            {
                // Create a TextWriter with null stream as a backing stream
                // which doesn't consume resources
                using (var nullWriter = new StreamWriter(Stream.Null))
                {
                    // Create a new HTTP context using the current URL.
                    var currentUrlHttpContext = mvcContextFactory.CreateHttpContext(null, currentUrl, nullWriter);

                    // Find node for the passed-in URL using the new HTTP context. This will do a
                    // match based on route values and/or query string values.
                    node = FindSiteMapNodeFromMvc(currentUrlHttpContext);
                }
            }

            return ReturnNodeIfAccessible(node);
        }

        /// <summary>
        /// Finds the site map node.
        /// </summary>
        /// <param name="context">The controller context.</param>
        /// <returns></returns>
        public virtual ISiteMapNode FindSiteMapNode(ControllerContext context)
        {
            return FindSiteMapNode(context.HttpContext);
        }

        /// <summary>
        /// Retrieves a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> object that represents the currently requested page using the current <see cref="T:System.Web.HttpContext"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> that represents the currently requested page; otherwise, <b>null</b>,
        /// if no corresponding <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> can be found in the <see cref="T:MvcSiteMapProvider.SiteMapNode"/> or if the page context is null.
        /// </returns>
        public virtual ISiteMapNode FindSiteMapNodeFromCurrentContext()
        {
            return FindSiteMapNode(HttpContext);
        }

        /// <summary>
        /// Retrieves a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> object based on a specified key.
        /// </summary>
        /// <param name="key">A lookup key with which a <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> instance is created.</param>
        /// <returns>A <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> that represents the page identified by key; otherwise, <b>null</b>,
        /// if no corresponding <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> is found or if security trimming is enabled and the
        /// <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> cannot be returned for the current user. The default is null.</returns>
        public virtual ISiteMapNode FindSiteMapNodeFromKey(string key)
        {
            ISiteMapNode node = null;
            if (keyTable.ContainsKey(key))
            {
                node = keyTable[key];
            }
            return ReturnNodeIfAccessible(node);
        }

        public virtual ISiteMapNodeCollection GetAncestors(ISiteMapNode node)
        {
            var ancestors = siteMapChildStateFactory.CreateSiteMapNodeCollection();
            GetAncestorsInternal(node, ancestors);
            return siteMapChildStateFactory.CreateReadOnlySiteMapNodeCollection(ancestors);
        }

        public virtual ISiteMapNodeCollection GetChildNodes(ISiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            ISiteMapNodeCollection collection = null;
            if (childNodeCollectionTable.ContainsKey(node))
            {
                collection = childNodeCollectionTable[node];
            }
            if (collection == null)
            {
                ISiteMapNode keyNode = null;
                if (keyTable.ContainsKey(node.Key))
                {
                    keyNode = keyTable[node.Key];
                }
                if (keyNode != null && childNodeCollectionTable.ContainsKey(keyNode))
                {
                    collection = childNodeCollectionTable[keyNode];
                }
            }
            if (collection == null)
            {
                return siteMapChildStateFactory.CreateEmptyReadOnlySiteMapNodeCollection();
            }
            if (!SecurityTrimmingEnabled)
            {
                return siteMapChildStateFactory.CreateReadOnlySiteMapNodeCollection(collection);
            }
            var secureCollection = siteMapChildStateFactory.CreateSiteMapNodeCollection();
            foreach (ISiteMapNode secureNode in collection)
            {
                if (secureNode.IsAccessibleToUser())
                {
                    secureCollection.Add(secureNode);
                }
            }
            return siteMapChildStateFactory.CreateReadOnlySiteMapNodeCollection(secureCollection);
        }

        public virtual ISiteMapNode GetCurrentNodeAndHintAncestorNodes(int upLevel)
        {
            return upLevel < -1 ? throw new ArgumentOutOfRangeException(nameof(upLevel)) : CurrentNode;
        }

        public virtual ISiteMapNode GetCurrentNodeAndHintNeighborhoodNodes(int upLevel, int downLevel)
        {
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(upLevel));
            }
            return downLevel < -1 ? throw new ArgumentOutOfRangeException(nameof(downLevel)) : CurrentNode;
        }

        public virtual ISiteMapNodeCollection GetDescendants(ISiteMapNode node)
        {
            var descendants = siteMapChildStateFactory.CreateSiteMapNodeCollection();
            GetDescendantsInternal(node, descendants);
            return siteMapChildStateFactory.CreateReadOnlySiteMapNodeCollection(descendants);
        }

        public virtual ISiteMapNode GetParentNode(ISiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            ISiteMapNode parentNode = null;
            if (parentNodeTable.ContainsKey(node))
            {
                parentNode = parentNodeTable[node];
            }
            if (parentNode == null)
            {
                ISiteMapNode keyNode = null;
                if (keyTable.ContainsKey(node.Key))
                {
                    keyNode = keyTable[node.Key];
                }
                if (keyNode != null)
                {
                    if (parentNodeTable.ContainsKey(keyNode))
                    {
                        parentNode = parentNodeTable[keyNode];
                    }
                }
            }
            return ReturnNodeIfAccessible(parentNode);
        }

        public virtual ISiteMapNode GetParentNodeRelativeToCurrentNodeAndHintDownFromParent(int walkupLevels, int relativeDepthFromWalkup)
        {
            if (walkupLevels < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(walkupLevels));
            }
            if (relativeDepthFromWalkup < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeDepthFromWalkup));
            }
            var currentNodeAndHintAncestorNodes = GetCurrentNodeAndHintAncestorNodes(walkupLevels);
            if (currentNodeAndHintAncestorNodes == null)
            {
                return null;
            }
            var parentNodesInternal = GetParentNodesInternal(currentNodeAndHintAncestorNodes, walkupLevels);
            if (parentNodesInternal == null)
            {
                return null;
            }
            HintNeighborhoodNodes(parentNodesInternal, 0, relativeDepthFromWalkup);
            return parentNodesInternal;
        }

        public virtual ISiteMapNode GetParentNodeRelativeToNodeAndHintDownFromParent(ISiteMapNode node, int walkupLevels, int relativeDepthFromWalkup)
        {
            if (walkupLevels < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(walkupLevels));
            }
            if (relativeDepthFromWalkup < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeDepthFromWalkup));
            }
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            HintAncestorNodes(node, walkupLevels);
            var parentNodesInternal = GetParentNodesInternal(node, walkupLevels);
            if (parentNodesInternal == null)
            {
                return null;
            }
            HintNeighborhoodNodes(parentNodesInternal, 0, relativeDepthFromWalkup);
            return parentNodesInternal;
        }

        public virtual void HintAncestorNodes(ISiteMapNode node, int upLevel)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(upLevel));
            }
        }

        public virtual void HintNeighborhoodNodes(ISiteMapNode node, int upLevel, int downLevel)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(upLevel));
            }
            if (downLevel < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(downLevel));
            }
        }

        /// <summary>
        /// Retrieves a Boolean value indicating whether the specified <see cref="T:MvcSiteMapProvider.SiteMapNode"/> object can be viewed by the user in the specified context.
        /// </summary>
        /// <param name="node">The <see cref="T:MvcSiteMapProvider.SiteMapNode"/> that is requested by the user.</param>
        /// <returns>
        /// true if security trimming is enabled and <paramref name="node"/> can be viewed by the user or security trimming is not enabled; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="node"/> is null.
        /// </exception>
        public virtual bool IsAccessibleToUser(ISiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            // If the sitemap is still being constructed, always
            // make all nodes accessible regardless of security trimming.
            if (!IsReadOnly)
            {
                return true;
            }
            return !SecurityTrimmingEnabled || pluginProvider.AclModule.IsAccessibleToUser(this, node);
        }

        public virtual void RemoveNode(ISiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            lock (synclock)
            {
                // Remove the parent node relationship
                ISiteMapNode parentNode = null;
                if (parentNodeTable.ContainsKey(node))
                {
                    parentNode = parentNodeTable[node];
                    parentNodeTable.Remove(node);
                }

                // Remove the child node relationship
                if (parentNode != null)
                {
                    var nodes = childNodeCollectionTable[parentNode];
                    if (nodes?.Contains(node) == true)
                    {
                        nodes.Remove(node);
                    }
                }

                // Remove the URL
                var url = siteMapChildStateFactory.CreateUrlKey(node);
                if (urlTable.ContainsKey(url))
                {
                    urlTable.Remove(url);
                }

                // Remove the key
                string key = node.Key;
                if (keyTable.ContainsKey(key))
                {
                    keyTable.Remove(key);
                }
            }
        }

        /// <summary>
        /// Resolves the action method parameters based on the current SiteMap instance.
        /// </summary>
        /// <remarks>There is 1 instance of action method parameter resolver per site map.</remarks>
        [Obsolete("ResolveActionMethodParameters is deprecated and will be removed in version 5.")]
        public IEnumerable<string> ResolveActionMethodParameters(string areaName, string controllerName, string actionMethodName)
        {
            return pluginProvider.MvcResolver.ResolveActionMethodParameters(areaName, controllerName, actionMethodName);
        }

        /// <summary>
        /// Resolves the controller type based on the current SiteMap instance.
        /// </summary>
        /// <remarks>There is 1 instance of controller type resolver per site map.</remarks>
        public Type ResolveControllerType(string areaName, string controllerName)
        {
            return pluginProvider.MvcResolver.ResolveControllerType(areaName, controllerName);
        }

        protected virtual void AddNodeInternal(ISiteMapNode node, ISiteMapNode parentNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            lock (synclock)
            {
                IUrlKey url = null;
                bool isMvcUrl = string.IsNullOrEmpty(node.UnresolvedUrl) && node.UsesDefaultUrlResolver();

                // Only store URLs if they are clickable and are configured using the Url
                // property or provided by a custom URL resolver.
                if (!isMvcUrl && node.Clickable)
                {
                    url = siteMapChildStateFactory.CreateUrlKey(node);

                    // Check for duplicates (including matching or empty host names).
                    if (urlTable
                        .Any(k => string.Equals(k.Key.RootRelativeUrl, url.RootRelativeUrl, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(k.Key.HostName) || string.IsNullOrEmpty(url.HostName) || string.Equals(k.Key.HostName, url.HostName, StringComparison.OrdinalIgnoreCase))))
                    {
                        var absoluteUrl = urlPath.ResolveUrl(node.UnresolvedUrl, string.IsNullOrEmpty(node.Protocol) ? Uri.UriSchemeHttp : node.Protocol, node.HostName);
                        throw new InvalidOperationException(string.Format(Resources.Messages.MultipleNodesWithIdenticalUrl, absoluteUrl));
                    }
                }

                // Add the key
                string key = node.Key;
                if (keyTable.ContainsKey(key))
                {
                    throw new InvalidOperationException(string.Format(Resources.Messages.MultipleNodesWithIdenticalKey, key));
                }
                keyTable[key] = node;

                // Add the URL
                if (url != null)
                {
                    urlTable[url] = node;
                }

                // Add the parent-child relationship
                if (parentNode != null)
                {
                    parentNodeTable[node] = parentNode;
                    if (!childNodeCollectionTable.ContainsKey(parentNode))
                    {
                        childNodeCollectionTable[parentNode] = siteMapChildStateFactory.CreateLockableSiteMapNodeCollection(this);
                    }
                    childNodeCollectionTable[parentNode].Add(node);
                }
            }
        }

        protected virtual void AssertSiteMapNodeConfigurationIsValid(ISiteMapNode node)
        {
            ThrowIfTitleNotSet(node);
            ThrowIfControllerNameInvalid(node);
            ThrowIfAreaNameInvalid(node);
            ThrowIfActionAndUrlNotSet(node);
            ThrowIfHttpMethodInvalid(node);
            ThrowIfRouteValueIsPreservedRouteParameter(node);
            ThrowIfHostNameInvalid(node);
            ThrowIfCanonicalUrlHostNameInvalid(node);
            ThrowIfImageUrlHostNameInvalid(node);
        }

        /// <summary>
        /// Finds the site map node.
        /// </summary>
        /// <param name="httpContext">The context.</param>
        /// <returns></returns>
        protected virtual ISiteMapNode FindSiteMapNode(HttpContextBase httpContext)
        {
            // Try URL
            var node = FindSiteMapNodeFromPublicFacingUrl(httpContext) ?? FindSiteMapNodeFromMvc(httpContext);

            // Check accessibility
            return ReturnNodeIfAccessible(node);
        }

        protected virtual ISiteMapNode FindSiteMapNodeFromMvc(HttpContextBase httpContext)
        {
            var routeData = GetMvcRouteData(httpContext);
            return routeData != null ? FindSiteMapNodeFromMvcRoute(routeData.Values, routeData.Route) : null;
        }

        /// <summary>
        /// Finds the node that matches the MVC route.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="route">The route.</param>
        /// <returns>
        /// A controller action node represented as a <see cref="SiteMapNode"/> instance
        /// </returns>
        protected virtual ISiteMapNode FindSiteMapNodeFromMvcRoute(IDictionary<string, object> values, RouteBase route)
        {
            var routes = mvcContextFactory.GetRoutes();

            // keyTable contains every node in the SiteMap
            foreach (var node in keyTable.Values)
            {
                // Look at the route property
                if (!string.IsNullOrEmpty(node.Route))
                {
                    // This looks a bit weird, but if I set up a node to a general route i.e. /Controller/Action/ID
                    // I need to check that the values are the same so that it doesn't swallow all of the nodes that also use that same general route
                    if (routes[node.Route] == route && node.MatchesRoute(values))
                    {
                        return node;
                    }
                }
                else if (node.MatchesRoute(values))
                {
                    return node;
                }
            }

            return null;
        }

        protected virtual ISiteMapNode FindSiteMapNodeFromPublicFacingUrl(HttpContextBase httpContext)
        {
            var publicFacingUrl = urlPath.GetPublicFacingUrl(httpContext);
            return FindSiteMapNodeFromUrl(publicFacingUrl.PathAndQuery, publicFacingUrl.AbsolutePath, publicFacingUrl.Host, httpContext.CurrentHandler);
        }

        protected virtual ISiteMapNode FindSiteMapNodeFromUrl(string relativeUrl, string relativePath, string hostName, IHttpHandler handler)
        {
            // Try absolute match with querystring
            var absoluteMatch = siteMapChildStateFactory.CreateUrlKey(relativeUrl, hostName);
            ISiteMapNode node = FindSiteMapNodeFromUrlMatch(absoluteMatch);

            // Try absolute match without querystring
            if (node == null && !string.IsNullOrEmpty(relativePath))
            {
                var absoluteMatchWithoutQueryString = siteMapChildStateFactory.CreateUrlKey(relativePath, hostName);
                node = FindSiteMapNodeFromUrlMatch(absoluteMatchWithoutQueryString);
            }

            // Try relative match
            if (node == null)
            {
                var relativeMatch = siteMapChildStateFactory.CreateUrlKey(relativeUrl, string.Empty);
                node = FindSiteMapNodeFromUrlMatch(relativeMatch);
            }

            // Try relative match with ASP.NET handler querystring
            if (node == null)
            {
                if (handler is Page currentHandler)
                {
                    string clientQueryString = currentHandler.ClientQueryString;
                    if (clientQueryString.Length > 0)
                    {
                        var aspNetRelativeMatch = siteMapChildStateFactory.CreateUrlKey(relativePath + "?" + clientQueryString, string.Empty);
                        node = FindSiteMapNodeFromUrlMatch(aspNetRelativeMatch);
                    }
                }
            }

            // Try relative match without querystring
            if (node == null && !string.IsNullOrEmpty(relativePath))
            {
                var relativeMatchWithoutQueryString = siteMapChildStateFactory.CreateUrlKey(relativePath, string.Empty);
                node = FindSiteMapNodeFromUrlMatch(relativeMatchWithoutQueryString);
            }

            return node;
        }

        protected virtual ISiteMapNode FindSiteMapNodeFromUrlMatch(IUrlKey urlToMatch)
        {
            return urlTable.ContainsKey(urlToMatch) ? urlTable[urlToMatch] : null;
        }

        protected virtual void GetAncestorsInternal(ISiteMapNode node, ISiteMapNodeCollection ancestors)
        {
            if (node.ParentNode != null)
            {
                ancestors.Add(node.ParentNode);
                GetAncestorsInternal(node.ParentNode, ancestors);
            }
        }

        protected virtual void GetDescendantsInternal(ISiteMapNode node, ISiteMapNodeCollection descendants)
        {
            foreach (var child in node.ChildNodes)
            {
                descendants.Add(child);
                GetDescendantsInternal(child, descendants);
            }
        }

        protected virtual RouteData GetMvcRouteData(HttpContextBase httpContext)
        {
            var routes = mvcContextFactory.GetRoutes();
            var routeData = routes.GetRouteData(httpContext);
            if (routeData != null)
            {
                if (routeData.Values.ContainsKey("MS_DirectRouteMatches"))
                {
                    routeData = ((IEnumerable<RouteData>)routeData.Values["MS_DirectRouteMatches"]).First();
                }
                SetMvcArea(routeData);
            }

            return routeData;
        }

        protected virtual ISiteMapNode GetParentNodesInternal(ISiteMapNode node, int walkupLevels)
        {
            if (walkupLevels > 0)
            {
                do
                {
                    node = node.ParentNode;
                    walkupLevels--;
                }
                while ((node != null) && (walkupLevels != 0));
            }
            return node;
        }

        protected virtual ISiteMapNode ReturnNodeIfAccessible(ISiteMapNode node)
        {
            return node?.IsAccessibleToUser() == true ? node : null;
        }

        protected virtual void SetMvcArea(RouteData routeData)
        {
            if (routeData != null)
            {
                if (!routeData.Values.ContainsKey("area"))
                {
                    routeData.Values.Add("area", routeData.GetAreaName());
                }
            }
        }

        protected virtual void ThrowIfActionAndUrlNotSet(ISiteMapNode node)
        {
            if (node.Clickable && string.IsNullOrEmpty(node.Action) && string.IsNullOrEmpty(node.UnresolvedUrl) && string.IsNullOrEmpty(node.Route))
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeActionAndRouteAndURLNotSet, node.Key, node.Title));
            }
        }

        protected virtual void ThrowIfAreaNameInvalid(ISiteMapNode node)
        {
            if (!string.IsNullOrEmpty(node.Area))
            {
                if (!node.Area.IsValidIdentifier())
                {
                    throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeAreaNameInvalid, node.Key, node.Title, node.Area));
                }
            }
        }

        protected virtual void ThrowIfCanonicalUrlHostNameInvalid(ISiteMapNode node)
        {
            if (!string.IsNullOrEmpty(node.CanonicalUrlHostName) && node.CanonicalUrlHostName.Contains(":"))
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeHostNameInvalid, node.Key, node.Title, node.CanonicalUrlHostName, "CanonicalUrlHostName"));
            }
        }

        protected virtual void ThrowIfControllerNameInvalid(ISiteMapNode node)
        {
            if (!string.IsNullOrEmpty(node.Controller))
            {
                if (!node.Controller.IsValidIdentifier() || node.Controller.EndsWith("Controller"))
                {
                    throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeControllerNameInvalid, node.Key, node.Title, node.Controller));
                }
            }
        }

        protected virtual void ThrowIfHostNameInvalid(ISiteMapNode node)
        {
            if (!string.IsNullOrEmpty(node.HostName) && node.HostName.Contains(":"))
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeHostNameInvalid, node.Key, node.Title, node.HostName, "HostName"));
            }
        }

        protected virtual void ThrowIfHttpMethodInvalid(ISiteMapNode node)
        {
            if (string.IsNullOrEmpty(node.HttpMethod) ||
                (!EnumHelper.TryParse<HttpVerbs>(node.HttpMethod, true, out _) &&
                !node.HttpMethod.Equals("*") &&
                !node.HttpMethod.Equals("Request", StringComparison.OrdinalIgnoreCase)))
            {
                var allowedVerbs = string.Join(Environment.NewLine, Enum.GetNames(typeof(HttpVerbs))) + Environment.NewLine + "Request" + Environment.NewLine + "*";
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeHttpMethodInvalid, node.Key, node.Title, node.HttpMethod, allowedVerbs));
            }
        }

        protected virtual void ThrowIfImageUrlHostNameInvalid(ISiteMapNode node)
        {
            if (!string.IsNullOrEmpty(node.ImageUrlHostName) && node.ImageUrlHostName.Contains(":"))
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeHostNameInvalid, node.Key, node.Title, node.ImageUrlHostName, "ImageUrlHostName"));
            }
        }

        protected virtual void ThrowIfRouteValueIsPreservedRouteParameter(ISiteMapNode node)
        {
            if (node.PreservedRouteParameters.Count > 0)
            {
                foreach (var key in node.PreservedRouteParameters)
                {
                    if (node.RouteValues.ContainsKey(key))
                        throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeSameKeyInRouteValueAndPreservedRouteParameter, node.Key, node.Title, key));
                }
            }
        }

        protected virtual void ThrowIfTitleNotSet(ISiteMapNode node)
        {
            if (string.IsNullOrEmpty(node.Title))
            {
                throw new MvcSiteMapException(string.Format(Resources.Messages.SiteMapNodeTitleNotSet, node.Key));
            }
        }
    }
}