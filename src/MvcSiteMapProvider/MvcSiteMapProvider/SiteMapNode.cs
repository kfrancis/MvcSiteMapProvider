using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider;

/// <summary>
///     SiteMapNode class. This class represents a node within the SiteMap hierarchy.
///     It contains all business logic to maintain the node's internal state.
/// </summary>
public class SiteMapNode
    : SiteMapNodePositioningBase, ISiteMapNode
{
    // Child collections and dictionaries
    private readonly IAttributeDictionary attributes;
    private readonly bool isDynamic;
    private readonly string key;
    private readonly ILocalizationService localizationService;
    private readonly IMetaRobotsValueCollection metaRobotsValues;
    private readonly IMvcContextFactory mvcContextFactory;

    // Services
    private readonly ISiteMapNodePluginProvider pluginProvider;
    private readonly IPreservedRouteParameterCollection preservedRouteParameters;
    private readonly IRoleCollection roles;
    private readonly IRouteValueDictionary routeValues;

    // Object State
    private readonly ISiteMap siteMap;
    private readonly IUrlPath urlPath;
    private string canonicalKey = string.Empty;
    private string canonicalUrl = string.Empty;
    private ChangeFrequency changeFrequency = ChangeFrequency.Undefined;
    private bool clickable = true;
    private string description = string.Empty;
    private string httpMethod = nameof(HttpVerbs.Get).ToUpperInvariant();
    private string imageUrl = string.Empty;
    private DateTime lastModifiedDate = DateTime.MinValue;
    private string resolvedUrl = string.Empty;
    private string title = string.Empty;
    private UpdatePriority updatePriority = UpdatePriority.Undefined;
    private string url = string.Empty;

    public SiteMapNode(
        ISiteMap siteMap,
        string key,
        bool isDynamic,
        ISiteMapNodePluginProvider pluginProvider,
        IMvcContextFactory mvcContextFactory,
        ISiteMapNodeChildStateFactory siteMapNodeChildStateFactory,
        ILocalizationService localizationService,
        IUrlPath urlPath
    )
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (siteMapNodeChildStateFactory == null)
        {
            throw new ArgumentNullException(nameof(siteMapNodeChildStateFactory));
        }

        this.siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
        this.key = key;
        this.isDynamic = isDynamic;
        this.pluginProvider = pluginProvider ?? throw new ArgumentNullException(nameof(pluginProvider));
        this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        this.localizationService =
            localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        this.urlPath = urlPath ?? throw new ArgumentNullException(nameof(urlPath));

        // Initialize child collections
        attributes =
            siteMapNodeChildStateFactory.CreateAttributeDictionary(key, "Attributes", siteMap, localizationService);
        routeValues = siteMapNodeChildStateFactory.CreateRouteValueDictionary(key, "RouteValues", siteMap);
        preservedRouteParameters = siteMapNodeChildStateFactory.CreatePreservedRouteParameterCollection(siteMap);
        roles = siteMapNodeChildStateFactory.CreateRoleCollection(siteMap);
        metaRobotsValues = siteMapNodeChildStateFactory.CreateMetaRobotsValueCollection(siteMap);
    }

    /// <summary>
    ///     Gets the current HTTP context.
    /// </summary>
    protected virtual HttpContextBase HttpContext => mvcContextFactory.CreateHttpContext();

    /// <summary>
    ///     Flag to ensure the route values are only preserved from the current request a single time.
    /// </summary>
    /// <returns><c>true</c> if the route values have been preserved for the current request; otherwise <c>false</c>.</returns>
    /// <remarks>This property must be overridden and provide an implementation that is stored in the request cache.</remarks>
    protected virtual bool AreRouteParametersPreserved
    {
        get => false;
        set { }
    }

    /// <summary>
    ///     Gets the key.
    /// </summary>
    /// <value>The key.</value>
    public override string Key => key;

    /// <summary>
    ///     Gets whether the current node was created from a dynamic source.
    /// </summary>
    /// <value>True if the current node is dynamic.</value>
    public override bool IsDynamic => isDynamic;

    /// <summary>
    ///     Gets whether the current node is read-only.
    /// </summary>
    /// <value>True if the current node is read-only.</value>
    public override bool IsReadOnly => SiteMap.IsReadOnly;

    /// <summary>
    ///     A reference to the root SiteMap object for the current graph.
    /// </summary>
    public override ISiteMap SiteMap => siteMap;

    /// <summary>
    ///     Gets or sets the HTTP method (such as GET, POST, or HEAD) to use to determine
    ///     node accessibility.
    /// </summary>
    /// <value>
    ///     The HTTP method.
    /// </value>
    public override string HttpMethod
    {
        get => httpMethod;
        set => httpMethod = value;
    }

    /// <summary>
    ///     Gets the implicit resource key (optional).
    /// </summary>
    /// <value>The implicit resource key.</value>
    public override string ResourceKey => localizationService.ResourceKey;

    /// <summary>
    ///     Gets or sets the title (optional).
    /// </summary>
    /// <value>The title.</value>
    /// <remarks>The title can be localized using a resource provider.</remarks>
    public override string Title
    {
        get => localizationService.GetResourceString("title", title, SiteMap);
        set => title = localizationService.ExtractExplicitResourceKey("title", value);
    }

    /// <summary>
    ///     Gets or sets the description (optional).
    /// </summary>
    /// <value>The description.</value>
    /// <remarks>The description can be localized using a resource provider.</remarks>
    public override string Description
    {
        get
        {
            var result = localizationService.GetResourceString("description", description, SiteMap);
            if (SiteMap.UseTitleIfDescriptionNotProvided && string.IsNullOrEmpty(result))
            {
                result = Title;
            }

            return result;
        }
        set => description = localizationService.ExtractExplicitResourceKey("description", value);
    }

    /// <summary>
    ///     Gets or sets the target frame (optional).
    /// </summary>
    /// <value>The target frame.</value>
    public override string TargetFrame { get; set; }

    /// <summary>
    ///     Gets or sets the image URL (optional).
    /// </summary>
    /// <value>The image URL.</value>
    /// <remarks>The image URL can be localized using a resource provider.</remarks>
    public override string ImageUrl
    {
        get
        {
            var imageUrl = localizationService.GetResourceString("imageUrl", this.imageUrl, SiteMap);
            return urlPath.ResolveContentUrl(imageUrl, ImageUrlProtocol, ImageUrlHostName);
        }
        set => imageUrl = localizationService.ExtractExplicitResourceKey("imageUrl", value);
    }

    /// <summary>
    ///     Gets or sets the image URL protocol, such as http, https (optional).
    ///     If not provided, it will default to the protocol of the current request.
    /// </summary>
    /// <value>The protocol of the image URL.</value>
    public override string ImageUrlProtocol { get; set; }

    /// <summary>
    ///     Gets or sets the image URL host name, such as www.somewhere.com (optional).
    /// </summary>
    /// <value>The protocol of the image URL.</value>
    public override string ImageUrlHostName { get; set; }

    /// <summary>
    ///     Gets the attributes (optional).
    /// </summary>
    /// <value>The attributes.</value>
    /// <remarks>The attributes can be localized using a resource provider.</remarks>
    public override IAttributeDictionary Attributes => attributes;

    /// <summary>
    ///     Gets the roles.
    /// </summary>
    /// <value>The roles.</value>
    public override IRoleCollection Roles => roles;

    /// <summary>
    ///     Gets or sets the last modified date.
    /// </summary>
    /// <value>The last modified date.</value>
    public override DateTime LastModifiedDate
    {
        get => lastModifiedDate;
        set => lastModifiedDate = value;
    }

    /// <summary>
    ///     Gets or sets the change frequency.
    /// </summary>
    /// <value>The change frequency.</value>
    public override ChangeFrequency ChangeFrequency
    {
        get => changeFrequency;
        set => changeFrequency = value;
    }

    /// <summary>
    ///     Gets or sets the update priority.
    /// </summary>
    /// <value>The update priority.</value>
    public override UpdatePriority UpdatePriority
    {
        get => updatePriority;
        set => updatePriority = value;
    }

    /// <summary>
    ///     Gets or sets the name or the type of the visibility provider.
    ///     This value will be used to select the concrete type of provider to use to determine
    ///     visibility.
    /// </summary>
    /// <value>
    ///     The name or type of the visibility provider.
    /// </value>
    public override string VisibilityProvider { get; set; }


    /// <summary>
    ///     Determines whether the node is visible.
    /// </summary>
    /// <param name="sourceMetadata">The source metadata.</param>
    /// <returns>
    ///     <c>true</c> if the specified node is visible; otherwise, <c>false</c>.
    /// </returns>
    public override bool IsVisible(IDictionary<string, object?> sourceMetadata)
    {
        // use strategy factory to provide implementation logic from concrete provider
        // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
        return pluginProvider.VisibilityProviderStrategy.IsVisible(VisibilityProvider, this, sourceMetadata);
    }

    /// <summary>
    ///     Gets or sets the name or type of the Dynamic Node Provider.
    /// </summary>
    /// <value>
    ///     The name or type of the Dynamic Node Provider.
    /// </value>
    public override string DynamicNodeProvider { get; set; }

    /// <summary>
    ///     Gets the dynamic node collection.
    /// </summary>
    /// <returns>A dynamic node collection.</returns>
    public override IEnumerable<DynamicNode> GetDynamicNodeCollection()
    {
        // use strategy factory to provide implementation logic from concrete provider
        // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
        return pluginProvider.DynamicNodeProviderStrategy.GetDynamicNodeCollection(DynamicNodeProvider, this);
    }

    /// <summary>
    ///     Gets whether the current node has a dynamic node provider.
    /// </summary>
    /// <value>
    ///     <c>true</c> if there is a provider; otherwise <c>false</c>.
    /// </value>
    public override bool HasDynamicNodeProvider =>
        // use strategy factory to provide implementation logic from concrete provider
        // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
        pluginProvider.DynamicNodeProviderStrategy.GetProvider(DynamicNodeProvider) != null;

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="SiteMapNode" /> is clickable.
    /// </summary>
    /// <value>
    ///     <c>true</c> if clickable; otherwise, <c>false</c>.
    /// </value>
    public override bool Clickable
    {
        get => clickable;
        set => clickable = value;
    }

    /// <summary>
    ///     Gets or sets the name or type of the URL resolver.
    /// </summary>
    /// <value>
    ///     The name or type of the URL resolver.
    /// </value>
    public override string UrlResolver { get; set; }

    /// <summary>
    ///     Gets the URL.
    /// </summary>
    /// <value>
    ///     The URL.
    /// </value>
    public override string Url
    {
        get
        {
            if (!Clickable)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(ResolvedUrl))
            {
                return ResolvedUrl;
            }

            return GetResolvedUrl();
        }
        set => url = value;
    }

    /// <summary>
    ///     The raw URL before being evaluated by any URL resolver.
    /// </summary>
    public override string UnresolvedUrl => url;

    /// <summary>
    ///     The resolved url that has been cached, if any.
    /// </summary>
    public override string ResolvedUrl => resolvedUrl;

    /// <summary>
    ///     A value indicating to cache the resolved URL. If false, the URL will be
    ///     resolved every time it is accessed.
    /// </summary>
    public override bool CacheResolvedUrl { get; set; }

    /// <summary>
    ///     Sets the ResolvedUrl using the current Url or Url resolver.
    /// </summary>
    public override void ResolveUrl()
    {
        var isProtocolOrHostNameFromRequest =
            !string.IsNullOrEmpty(Protocol) && (string.IsNullOrEmpty(HostName) || Protocol == "*");

        // NOTE: In all cases where values from the current request can be included in the URL, 
        // we need to disable URL resolution caching.
        if (CacheResolvedUrl &&
            string.IsNullOrEmpty(UnresolvedUrl) &&
            preservedRouteParameters.Count == 0 &&
            !IncludeAmbientValuesInUrl &&
            !isProtocolOrHostNameFromRequest)
        {
            resolvedUrl = GetResolvedUrl();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to include ambient request values
    ///     (from the RouteValues and/or query string) when resolving URLs.
    /// </summary>
    /// <value><b>true</b> to include ambient values (like MVC does); otherwise <b>false</b>.</value>
    public override bool IncludeAmbientValuesInUrl { get; set; }

    /// <summary>
    ///     Gets or sets the protocol, such as http or https that will
    ///     be built into the URL.
    /// </summary>
    /// <value>The protocol.</value>
    public override string Protocol { get; set; }

    /// <summary>
    ///     Gets or sets the host name that will be built into the URL.
    /// </summary>
    /// <value>The host name.</value>
    public override string HostName { get; set; }

    /// <summary>
    ///     Gets a boolean value that indicates this is an external URL by checking whether it
    ///     looks like an absolute path.
    /// </summary>
    /// <returns></returns>
    public override bool HasAbsoluteUrl()
    {
        return urlPath.IsAbsoluteUrl(Url);
    }

    /// <summary>
    ///     Gets a boolean value that indicates this is an external URL by checking whether it
    ///     looks like an absolute path and comparing the DnsSafeHost with the passed in context.
    /// </summary>
    /// <param name="httpContext">The http context for the current request.</param>
    /// <returns></returns>
    public override bool HasExternalUrl(HttpContextBase httpContext)
    {
        return urlPath.IsExternalUrl(Url, httpContext);
    }

    /// <summary>
    ///     Gets or sets the canonical key. The key is used to reference another
    ///     <see cref="T:MvcSiteMapProvider.ISiteMapNode" /> to get the canonical URL.
    /// </summary>
    /// <remarks>May not be used in conjunction with CanonicalUrl. Only 1 canonical value is allowed.</remarks>
    public override string CanonicalKey
    {
        get => canonicalKey;
        set
        {
            if (!canonicalKey.Equals(value))
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(canonicalUrl))
                {
                    throw new ArgumentException(
                        string.Format(Messages.SiteMapNodeCanonicalValueAlreadySet, "CanonicalKey"),
                        "CanonicalKey");
                }

                canonicalKey = value;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the canonical URL.
    /// </summary>
    /// <remarks>May not be used in conjunction with CanonicalKey. Only 1 canonical value is allowed.</remarks>
    public override string CanonicalUrl
    {
        get
        {
            var absoluteCanonicalUrl = GetAbsoluteCanonicalUrl();
            if (!string.IsNullOrEmpty(absoluteCanonicalUrl))
            {
                var publicFacingUrl = urlPath.GetPublicFacingUrl(HttpContext);
                if (absoluteCanonicalUrl.Equals(urlPath.UrlDecode(publicFacingUrl.AbsoluteUri)))
                {
                    return string.Empty;
                }
            }

            return absoluteCanonicalUrl;
        }
        set
        {
            if (!canonicalUrl.Equals(value))
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(canonicalKey))
                {
                    throw new ArgumentException(
                        string.Format(Messages.SiteMapNodeCanonicalValueAlreadySet, "CanonicalUrl"),
                        "CanonicalUrl");
                }

                canonicalUrl = value;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the canonical URL protocol, such as http, https (optional).
    /// </summary>
    /// <value>The protocol of the image URL.</value>
    public override string CanonicalUrlProtocol { get; set; }

    /// <summary>
    ///     Gets or sets the canonical URL host name, such as www.somewhere.com (optional).
    /// </summary>
    /// <value>The protocol of the image URL.</value>
    public override string CanonicalUrlHostName { get; set; }

    /// <summary>
    ///     Gets the robots meta values.
    /// </summary>
    /// <value>The robots meta values.</value>
    public override IMetaRobotsValueCollection MetaRobotsValues => metaRobotsValues;

    /// <summary>
    ///     Gets a string containing the pre-formatted comma delimited list of values that can be inserted into the
    ///     content attribute of the meta robots tag.
    /// </summary>
    public override string GetMetaRobotsContentString()
    {
        return MetaRobotsValues.GetMetaRobotsContentString();
    }

    /// <summary>
    ///     Gets a boolean value indicating whether both the noindex and nofollow values are included in the
    ///     list of robots meta values.
    /// </summary>
    public override bool HasNoIndexAndNoFollow => MetaRobotsValues.HasNoIndexAndNoFollow;

    /// <summary>
    ///     Gets or sets the route.
    /// </summary>
    /// <value>The route.</value>
    public override string Route { get; set; }

    /// <summary>
    ///     Gets the route values.
    /// </summary>
    /// <value>The route values.</value>
    public override IRouteValueDictionary RouteValues
    {
        get
        {
            if (IsReadOnly && !AreRouteParametersPreserved)
            {
                PreserveRouteParameters();
                AreRouteParametersPreserved = true;
            }

            return routeValues;
        }
    }

    /// <summary>
    ///     Gets the preserved route parameter names (= values that will be used from the current request route).
    /// </summary>
    /// <value>The preserved route parameters.</value>
    public override IPreservedRouteParameterCollection PreservedRouteParameters => preservedRouteParameters;

    /// <summary>
    ///     Gets the route data associated with the current node.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The route data associated with the current node.</returns>
    public override RouteData GetRouteData(HttpContextBase httpContext)
    {
        var routes = mvcContextFactory.GetRoutes();
        RouteData routeData;
        routeData = !string.IsNullOrEmpty(Route) ? routes[Route].GetRouteData(httpContext) : routes.GetRouteData(httpContext);

        return routeData;
    }

    /// <summary>
    ///     Determines whether this node matches the supplied route values.
    /// </summary>
    /// <param name="routeValues">An <see cref="T:System.Collections.Generic.IDictionary{string, object}" /> of route values.</param>
    /// <returns><c>true</c> if the route matches this node's RouteValues collection; otherwise <c>false</c>.</returns>
    public override bool MatchesRoute(IDictionary<string, object> routeValues)
    {
        // If not clickable, we never want to match the node.
        if (!Clickable)
        {
            return false;
        }

        // If URL is set explicitly, we should never match based on route values.
        if (!string.IsNullOrEmpty(UnresolvedUrl))
        {
            return false;
        }

        // Check whether the configured host name matches (only if it is supplied).
        if (!string.IsNullOrEmpty(HostName) && !urlPath.IsPublicHostName(HostName, HttpContext))
        {
            return false;
        }

        // Merge in any query string values from the current context that match keys with
        // the route values configured in the current node (MVC doesn't automatically assign them 
        // as route values). This allows matching on query string values, but only if they 
        // are configured in the node.
        var values = MergeRouteValuesAndNamedQueryStringValues(routeValues, RouteValues.Keys, HttpContext);

        return RouteValues.MatchesRoute(values);
    }

    /// <summary>
    ///     Gets or sets the area.
    /// </summary>
    /// <value>The area.</value>
    public override string Area
    {
        get => RouteValues.ContainsKey("area") && RouteValues["area"] != null
            ? RouteValues["area"].ToString()
            : string.Empty;
        set => RouteValues["area"] = value;
    }

    /// <summary>
    ///     Gets or sets the controller.
    /// </summary>
    /// <value>The controller.</value>
    public override string Controller
    {
        get => RouteValues.ContainsKey("controller") && RouteValues["controller"] != null
            ? RouteValues["controller"].ToString()
            : string.Empty;
        set => RouteValues["controller"] = value;
    }

    /// <summary>
    ///     Gets or sets the action.
    /// </summary>
    /// <value>The action.</value>
    public override string Action
    {
        get => RouteValues.ContainsKey("action") && RouteValues["action"] != null
            ? RouteValues["action"].ToString()
            : string.Empty;
        set => RouteValues["action"] = value;
    }

    public override void CopyTo(ISiteMapNode node)
    {
        // NOTE: Expected behavior is to reference 
        // the same child nodes, so this is okay.
        if (node.ChildNodes != null) // Guard for mocks or incomplete implementations
        {
            foreach (var child in ChildNodes)
            {
                node.ChildNodes.Add(child);
            }
        }

        node.HttpMethod = HttpMethod;
        node.Title = title; // Get protected member
        node.Description = description; // Get protected member
        node.TargetFrame = TargetFrame;
        node.ImageUrl = ImageUrl;
        node.ImageUrlProtocol = ImageUrlProtocol;
        node.ImageUrlHostName = ImageUrlHostName;
        if (node.Attributes != null) // Guard for mocks
        {
            Attributes.CopyTo(node.Attributes);
        }
        if (node.Roles != null)
        {
            Roles.CopyTo(node.Roles);
        }
        node.LastModifiedDate = LastModifiedDate;
        node.ChangeFrequency = ChangeFrequency;
        node.UpdatePriority = UpdatePriority;
        node.VisibilityProvider = VisibilityProvider;
        node.Clickable = Clickable;
        node.UrlResolver = UrlResolver;
        node.Url = url; // Get protected member
        node.CacheResolvedUrl = CacheResolvedUrl;
        node.IncludeAmbientValuesInUrl = IncludeAmbientValuesInUrl;
        node.Protocol = Protocol;
        node.HostName = HostName;
        node.CanonicalKey = CanonicalKey;
        node.CanonicalUrl = canonicalUrl; // Get protected member
        node.CanonicalUrlProtocol = CanonicalUrlProtocol;
        node.CanonicalUrlHostName = CanonicalUrlHostName;
        if (node.MetaRobotsValues != null)
        {
            MetaRobotsValues.CopyTo(node.MetaRobotsValues);
        }
        node.DynamicNodeProvider = DynamicNodeProvider;
        node.Route = Route;
        if (node.RouteValues != null)
        {
            RouteValues.CopyTo(node.RouteValues);
        }
        if (node.PreservedRouteParameters != null)
        {
            PreservedRouteParameters.CopyTo(node.PreservedRouteParameters);
        }
        // NOTE: Area, Controller, and Action are covered under RouteValues.
    }

    public override bool Equals(ISiteMapNode node)
    {
        if (base.Equals((object)node))
        {
            return true;
        }

        if (node == null)
        {
            return false;
        }

        return Key.Equals(node.Key);
    }

    protected string GetResolvedUrl()
    {
        // use strategy factory to provide implementation logic from concrete provider
        // http://stackoverflow.com/questions/1499442/best-way-to-use-structuremap-to-implement-strategy-pattern
        return pluginProvider.UrlResolverStrategy.ResolveUrl(
            UrlResolver, this, Area, Controller, Action, RouteValues);
    }

    /// <summary>
    ///     Gets the absolute value of the canonical URL, finding the value by
    ///     <see cref="P:MvcSiteMapProvider.ISiteMapNode.CanonicalKey" /> if necessary.
    /// </summary>
    /// <returns>The absolute canonical URL.</returns>
    protected virtual string GetAbsoluteCanonicalUrl()
    {
        var url = canonicalUrl;
        if (!string.IsNullOrEmpty(url))
        {
            // Use HTTP if not provided to force an absolute URL to be built.
            var protocol = string.IsNullOrEmpty(CanonicalUrlProtocol) ? Uri.UriSchemeHttp : CanonicalUrlProtocol;
            return urlPath.ResolveUrl(url, protocol, CanonicalUrlHostName);
        }

        var key = canonicalKey;
        if (!string.IsNullOrEmpty(key))
        {
            var node = SiteMap.FindSiteMapNodeFromKey(key);
            if (node != null)
            {
                // Use HTTP if not provided to force an absolute URL to be built.
                var protocol = string.IsNullOrEmpty(node.Protocol) ? Uri.UriSchemeHttp : node.Protocol;
                return urlPath.ResolveUrl(node.Url, protocol, node.HostName);
            }
        }

        return string.Empty;
    }

    /// <summary>
    ///     Sets the preserved route parameters of the current request to the routeValues collection.
    /// </summary>
    /// <remarks>
    ///     This method relies on the fact that the route value collection is request cached. The
    ///     values written are for the current request only, after which they will be discarded.
    /// </remarks>
    protected virtual void PreserveRouteParameters()
    {
        if (PreservedRouteParameters.Count > 0)
        {
            var requestContext = mvcContextFactory.CreateRequestContext();
            var routeDataValues = requestContext.RouteData.Values;
            var queryStringValues = GetCaseCorrectedQueryString(requestContext.HttpContext);

            foreach (var item in PreservedRouteParameters)
            {
                var preservedParameterName = item.Trim();
                if (!string.IsNullOrEmpty(preservedParameterName))
                {
                    if (routeDataValues.TryGetValue(preservedParameterName, out var value))
                    {
                        routeValues[preservedParameterName] =
                            value;
                    }
                    else if (queryStringValues[preservedParameterName] != null)
                    {
                        routeValues[preservedParameterName] =
                            queryStringValues[preservedParameterName];
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Gets a <see cref="T:System.Collections.Specialized.NameValueCollection" /> containing the query string
    ///     key value pairs for the passed in HTTP context. The casing of the keys corrected to be the same case as the values
    ///     that are
    ///     configured either in the <see cref="P:RouteValues" /> dictionary or the <see cref="P:PreservedRouteParameters" />
    ///     collection.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>
    ///     A <see cref="T:System.Collections.Specialized.NameValueCollection" /> containing the case-corrected
    ///     key value pairs of the query string.
    /// </returns>
    protected virtual NameValueCollection GetCaseCorrectedQueryString(HttpContextBase httpContext)
    {
        var queryStringValues = httpContext.Request.QueryString;
        // Note: we must use the configured route values, rather than the RouteValue property to avoid an
        // infinite loop.
        var routeKeys = routeValues.Keys.ToArray();
        var caseInsensitiveRouteKeys = new HashSet<string>(routeKeys, StringComparer.InvariantCultureIgnoreCase);
        var caseInsensitivePreservedRouteParameters =
            new HashSet<string>(PreservedRouteParameters, StringComparer.InvariantCultureIgnoreCase);
        var result = new NameValueCollection(queryStringValues.Count);

        foreach (var key in queryStringValues.AllKeys)
        {
            // A malformed URL could have a null key
            if (key != null)
            {
                if (caseInsensitivePreservedRouteParameters.Contains(key))
                {
                    result.AddWithCaseCorrection(key, queryStringValues[key], PreservedRouteParameters);
                }
                else if (caseInsensitiveRouteKeys.Contains(key))
                {
                    result.AddWithCaseCorrection(key, queryStringValues[key], routeKeys);
                }
                else
                {
                    // If the value is not configured, add it to the dictionary with the original case.
                    result.Add(key, queryStringValues[key]);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Makes a copy of the passed in route values and merges in any query string parameters
    ///     that are configured for the current node. This ensures query string parameters are only
    ///     taken into consideration for the match.
    /// </summary>
    /// <param name="routeValues">The route values from the RouteData object.</param>
    /// <param name="queryStringKeys">
    ///     A list of keys of query string values to add to the route values if
    ///     they exist in the current context.
    /// </param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>
    ///     A merged list of routeValues and query string values. Route values will take precedence
    ///     over query string values in cases where both are specified.
    /// </returns>
    protected virtual IDictionary<string, object> MergeRouteValuesAndNamedQueryStringValues(
        IDictionary<string, object> routeValues, ICollection<string> queryStringKeys, HttpContextBase httpContext)
    {
        // Make a copy of the routeValues. We only want to limit this to the scope of the current node.
        var result = new Dictionary<string, object>(routeValues);

        // Add any query string values from the current context
        var queryStringValues = GetCaseCorrectedQueryString(httpContext);

        // QueryString collection might contain nullable keys
        foreach (var key in queryStringValues.AllKeys)
        {
            // Copy the query string value as a route value if it doesn't already exist
            // and the name is provided as a match. Note that route values will take
            // precedence over query string parameters in cases of duplicates
            // (unless the route value contains an empty value, then overwrite).
            if (key != null &&
                queryStringKeys.Contains(key) &&
                (!result.ContainsKey(key) || string.IsNullOrEmpty(result[key].ToString())))
            {
                result[key] = queryStringValues[key];
            }
        }

        return result;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is ISiteMapNode node))
        {
            return false;
        }

        return Equals(node);
    }

    public static bool operator ==(SiteMapNode node1, SiteMapNode node2)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(node1, node2))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if ((object)node1 == null || (object)node2 == null)
        {
            return false;
        }

        return node1.Equals(node2);
    }

    public static bool operator !=(SiteMapNode node1, SiteMapNode node2)
    {
        return !(node1 == node2);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    public override string ToString()
    {
        return Key;
    }
}
