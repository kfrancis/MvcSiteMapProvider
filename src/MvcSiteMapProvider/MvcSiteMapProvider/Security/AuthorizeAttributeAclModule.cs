using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Routing;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Mvc.Filters;
using MvcSiteMapProvider.Web.Routing;

namespace MvcSiteMapProvider.Security;

/// <summary>
///     An ACL module that determines whether the current user has access to a given node based on the MVC
///     AuthorizeAttribute.
/// </summary>
public class AuthorizeAttributeAclModule
    : IAclModule
{
    public AuthorizeAttributeAclModule(
        IMvcContextFactory mvcContextFactory,
        IControllerDescriptorFactory controllerDescriptorFactory,
        IControllerBuilder controllerBuilder,
        IGlobalFilterProvider filterProvider
    )
    {
        _mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        _controllerDescriptorFactory = controllerDescriptorFactory ??
                                           throw new ArgumentNullException(nameof(controllerDescriptorFactory));
        _controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
        _filterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
    }

    private readonly IMvcContextFactory _mvcContextFactory;
    private readonly IControllerDescriptorFactory _controllerDescriptorFactory;
    private readonly IControllerBuilder _controllerBuilder;
    private readonly IGlobalFilterProvider _filterProvider;

    /// <summary>
    ///     Determines whether node is accessible to user.
    /// </summary>
    /// <param name="siteMap">The site map.</param>
    /// <param name="node">The node.</param>
    /// <returns>
    ///     <c>true</c> if accessible to user; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAccessibleToUser(ISiteMap siteMap, ISiteMapNode node)
    {
        // Not Clickable? Always accessible.
        if (!node.Clickable)
        {
            return true;
        }

        var httpContext = _mvcContextFactory.CreateHttpContext();

        // Is it an external Url?
        return node.HasExternalUrl(httpContext) || VerifyNode(siteMap, node, httpContext);
    }

    protected virtual bool VerifyNode(ISiteMap siteMap, ISiteMapNode node, HttpContextBase httpContext)
    {
        var routes = FindRoutesForNode(node, httpContext);
        if (routes == null)
        {
            return true; // Static URLs will sometimes have no route data, therefore return true.
        }

        // Time to delve into the AuthorizeAttribute defined on the node.
        // Let's start by getting all metadata for the controller...
        var controllerType =
            siteMap.ResolveControllerType(routes.GetAreaName(), routes.GetOptionalString("controller"));
        if (controllerType == null)
        {
            return true;
        }

        return VerifyController(node, routes, controllerType);
    }

    protected virtual bool VerifyController(ISiteMapNode node, RouteData routes, Type controllerType)
    {
        // Get controller factory
        var controllerFactory = _controllerBuilder.GetControllerFactory();

        // Try to create controller context
        ControllerContext? controllerContext = null;
        bool factoryBuiltController = false;
        
        try
        {
            controllerContext = CreateControllerContext(node, routes, controllerType, controllerFactory,
                out factoryBuiltController);
        }
        catch
        {
            // If we can't create a controller context (e.g., controller has dependencies we can't resolve),
            // we allow access rather than failing. This is consistent with returning true when
            // controller type is null or action descriptor is null.
            return true;
        }

        try
        {
            return VerifyControllerAttributes(routes, controllerType, controllerContext);
        }
        finally
        {
            // Release controller
            if (factoryBuiltController && controllerContext != null)
            {
                controllerFactory.ReleaseController(controllerContext.Controller);
            }
        }
    }

    protected virtual RouteData? FindRoutesForNode(ISiteMapNode node, HttpContextBase httpContext)
    {
        // If the node is an MVC node, don't route-match. Just materialize RouteData.
        if (TryGetMvcNodeValues(node, out var area, out var controller, out var action))
        {
            return CreateRouteData(area, controller!, action!);
        }

        // Fallback for truly "URL-only" nodes
        return FindRoutesForNodeByUrl(node, httpContext);
    }

    protected virtual RouteData? FindRoutesForNodeByUrl(ISiteMapNode node, HttpContextBase httpContext)
    {
        // Use per-request cache in HttpContext.Items for URL-only nodes
        // This is safe across different sitemap configurations per user type
        var cache = (Dictionary<string, RouteData?>)(httpContext.Items["MSMP.RouteDataCache"]
            ??= new Dictionary<string, RouteData?>(StringComparer.OrdinalIgnoreCase));

        // Use node.Url as the cache key (stable per request)
        var key = node.Url;
        if (cache.TryGetValue(key, out var hit))
        {
            return hit;
        }

        // Create a Uri for the current node. If we have an absolute URL,
        // it will be used instead of the baseUri.
        var nodeUri = new Uri(httpContext.Request.Url, node.Url);

        // TextWriter.Null: avoid allocating StreamWriter each call
        var nodeHttpContext = _mvcContextFactory.CreateHttpContext(node, nodeUri, TextWriter.Null);

        // Find routes for the sitemap node's URL using the new HTTP context
        var routeData = node.GetRouteData(nodeHttpContext);

        // Cache the result (including null) to avoid repeated misses
        cache[key] = routeData;

        return routeData;
    }

    protected virtual bool TryGetMvcNodeValues(ISiteMapNode node, out string? area, out string? controller, out string? action)
    {
        area = null;
        controller = null;
        action = null;

        // Check if the node has controller and action defined (MVC node)
        controller = node.Controller;
        action = node.Action;

        // Must have at least controller and action to be considered an MVC node
        if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action))
        {
            return false;
        }

        // Area is optional
        area = node.Area;

        return true;
    }

    protected static RouteData CreateRouteData(string? area, string controller, string action)
    {
        var rd = new RouteData();
        rd.Values["controller"] = controller;
        rd.Values["action"] = action;

        if (!string.IsNullOrEmpty(area))
        {
            rd.DataTokens["area"] = area; // what GetAreaName() typically reads
        }

        return rd;
    }

    protected virtual bool VerifyControllerAttributes(RouteData routes, Type controllerType,
        ControllerContext controllerContext)
    {
        // Get controller descriptor
        var controllerDescriptor = _controllerDescriptorFactory.Create(controllerType);
        if (controllerDescriptor == null)
        {
            return true;
        }

        // Get action descriptor
        var actionDescriptor = GetActionDescriptor(routes.GetOptionalString("action"), controllerDescriptor,
            controllerContext);
        if (actionDescriptor == null)
        {
            return true;
        }

        // Verify security
        var authorizeAttributes = GetAuthorizeAttributes(actionDescriptor, controllerContext);
        return VerifyAuthorizeAttributes(authorizeAttributes, controllerContext, actionDescriptor);
    }

    protected virtual bool VerifyAuthorizeAttributes(IEnumerable<AuthorizeAttribute?> authorizeAttributes,
        ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    {
        // Verify all attributes
        foreach (var authorizeAttribute in authorizeAttributes)
        {
            try
            {
                var authorized = VerifyAuthorizeAttribute(authorizeAttribute, controllerContext, actionDescriptor);
                if (!authorized)
                {
                    return false;
                }
            }
            catch
            {
                // do not allow on exception
                return false;
            }
        }

        return true;
    }

#if MVC2
        protected virtual IEnumerable<AuthorizeAttribute> GetAuthorizeAttributes(ActionDescriptor actionDescriptor, ControllerContext controllerContext)
        {
            return actionDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType
                    <AuthorizeAttribute>().ToList()
                .Union(
                    actionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AuthorizeAttribute), true).OfType
                        <AuthorizeAttribute>().ToList());
        }
#else
    protected virtual IEnumerable<AuthorizeAttribute?> GetAuthorizeAttributes(ActionDescriptor actionDescriptor,
        ControllerContext controllerContext)
    {
        var filters = _filterProvider.GetFilters(controllerContext, actionDescriptor);

        return filters
            .Where(f => typeof(AuthorizeAttribute).IsAssignableFrom(f.Instance.GetType()))
            .Select(f => f.Instance as AuthorizeAttribute);
    }
#endif

    protected virtual bool VerifyAuthorizeAttribute(AuthorizeAttribute authorizeAttribute,
        ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    {
        var authorizationContext =
            _mvcContextFactory.CreateAuthorizationContext(controllerContext, actionDescriptor);
        authorizeAttribute.OnAuthorization(authorizationContext);
        return authorizationContext.Result == null;
    }

    protected virtual ActionDescriptor? GetActionDescriptor(string actionName,
        ControllerDescriptor controllerDescriptor, ControllerContext controllerContext)
    {
        var found = TryFindActionDescriptor(actionName, controllerContext, controllerDescriptor,
            out var actionDescriptor);

        if (!found)
        {
            actionDescriptor = controllerDescriptor.GetCanonicalActions().DefaultIfEmpty(null)
                .FirstOrDefault(a => a?.ActionName == actionName);
        }

        return actionDescriptor;
    }

    protected virtual bool TryFindActionDescriptor(string actionName, ControllerContext controllerContext,
        ControllerDescriptor controllerDescriptor, out ActionDescriptor? actionDescriptor)
    {
        actionDescriptor = null;
        try
        {
            var actionSelector = new ActionSelector();
            actionDescriptor = actionSelector.FindAction(controllerContext, controllerDescriptor, actionName);
            if (actionDescriptor != null)
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    protected virtual ControllerContext CreateControllerContext(ISiteMapNode node, RouteData routes,
        Type controllerType, IControllerFactory controllerFactory, out bool factoryBuiltController)
    {
        var requestContext = _mvcContextFactory.CreateRequestContext(node, routes);
        var controllerName = requestContext.RouteData.GetOptionalString("controller");

        // Whether controller is built by the ControllerFactory (or otherwise by Activator)
        factoryBuiltController =
            TryCreateController(requestContext, controllerName, controllerFactory, out var controller);
        if (!factoryBuiltController)
        {
            TryCreateController(controllerType, out controller);
        }

        // Create controller context
        return _mvcContextFactory.CreateControllerContext(requestContext, controller!);
    }

    protected virtual bool TryCreateController(RequestContext requestContext, string controllerName,
        IControllerFactory? controllerFactory, out ControllerBase? controller)
    {
        controller = null;
        if (controllerFactory == null)
        {
            return false;
        }

        try
        {
            controller = controllerFactory.CreateController(requestContext, controllerName) as ControllerBase;
            if (controller != null)
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    protected virtual bool TryCreateController(Type controllerType, out ControllerBase? controller)
    {
        controller = null;
        try
        {
            controller = Activator.CreateInstance(controllerType) as ControllerBase;
            if (controller != null)
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private class ActionSelector
        : AsyncControllerActionInvoker
    {
        // Needed because FindAction is protected, and we are changing it to be public
        public new ActionDescriptor? FindAction(ControllerContext controllerContext,
            ControllerDescriptor controllerDescriptor, string actionName)
        {
            return base.FindAction(controllerContext, controllerDescriptor, actionName);
        }
    }
}
