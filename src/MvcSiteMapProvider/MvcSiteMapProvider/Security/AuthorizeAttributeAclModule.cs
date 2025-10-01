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
        this.mvcContextFactory = mvcContextFactory ?? throw new ArgumentNullException(nameof(mvcContextFactory));
        this.controllerDescriptorFactory = controllerDescriptorFactory ??
                                           throw new ArgumentNullException(nameof(controllerDescriptorFactory));
        this.controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
        this.filterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
    }

    private readonly IMvcContextFactory mvcContextFactory;
    private readonly IControllerDescriptorFactory controllerDescriptorFactory;
    private readonly IControllerBuilder controllerBuilder;
    private readonly IGlobalFilterProvider filterProvider;

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

        var httpContext = mvcContextFactory.CreateHttpContext();

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
        var controllerFactory = controllerBuilder.GetControllerFactory();

        // Create controller context
        var controllerContext = CreateControllerContext(node, routes, controllerType, controllerFactory,
            out var factoryBuiltController);
        try
        {
            return VerifyControllerAttributes(routes, controllerType, controllerContext);
        }
        finally
        {
            // Release controller
            if (factoryBuiltController)
            {
                controllerFactory.ReleaseController(controllerContext.Controller);
            }
        }
    }

    protected virtual RouteData FindRoutesForNode(ISiteMapNode node, HttpContextBase httpContext)
    {
        // Create a Uri for the current node. If we have an absolute URL,
        // it will be used instead of the baseUri.
        var nodeUri = new Uri(httpContext.Request.Url, node.Url);

        // Create a TextWriter with null stream as a backing stream 
        // which doesn't consume resources
        using var nullWriter = new StreamWriter(Stream.Null);

        // Create a new HTTP context using the node's URL instead of the current one.
        var nodeHttpContext = mvcContextFactory.CreateHttpContext(node, nodeUri, nullWriter);

        // Find routes for the sitemap node's URL using the new HTTP context
        var routeData = node.GetRouteData(nodeHttpContext);

        return routeData;
    }

    protected virtual bool VerifyControllerAttributes(RouteData routes, Type controllerType,
        ControllerContext controllerContext)
    {
        // Get controller descriptor
        var controllerDescriptor = controllerDescriptorFactory.Create(controllerType);
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
        var filters = filterProvider.GetFilters(controllerContext, actionDescriptor);

        return filters
            .Where(f => typeof(AuthorizeAttribute).IsAssignableFrom(f.Instance.GetType()))
            .Select(f => f.Instance as AuthorizeAttribute);
    }
#endif

    protected virtual bool VerifyAuthorizeAttribute(AuthorizeAttribute authorizeAttribute,
        ControllerContext controllerContext, ActionDescriptor actionDescriptor)
    {
        var authorizationContext =
            mvcContextFactory.CreateAuthorizationContext(controllerContext, actionDescriptor);
        authorizeAttribute.OnAuthorization(authorizationContext);
        return authorizationContext.Result == null;
    }

    protected virtual ActionDescriptor GetActionDescriptor(string actionName,
        ControllerDescriptor controllerDescriptor, ControllerContext controllerContext)
    {
        var found = TryFindActionDescriptor(actionName, controllerContext, controllerDescriptor,
            out var actionDescriptor);
        if (!found)
        {
            actionDescriptor = controllerDescriptor.GetCanonicalActions()
                .FirstOrDefault(a => a.ActionName == actionName);
        }

        return actionDescriptor;
    }

    protected virtual bool TryFindActionDescriptor(string actionName, ControllerContext controllerContext,
        ControllerDescriptor controllerDescriptor, out ActionDescriptor actionDescriptor)
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
        var requestContext = mvcContextFactory.CreateRequestContext(node, routes);
        var controllerName = requestContext.RouteData.GetOptionalString("controller");

        // Whether controller is built by the ControllerFactory (or otherwise by Activator)
        factoryBuiltController =
            TryCreateController(requestContext, controllerName, controllerFactory, out var controller);
        if (!factoryBuiltController)
        {
            TryCreateController(controllerType, out controller);
        }

        // Create controller context
        var controllerContext = mvcContextFactory.CreateControllerContext(requestContext, controller);
        return controllerContext;
    }

    protected virtual bool TryCreateController(RequestContext requestContext, string controllerName,
        IControllerFactory controllerFactory, out ControllerBase? controller)
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
        public new ActionDescriptor FindAction(ControllerContext controllerContext,
            ControllerDescriptor controllerDescriptor, string actionName)
        {
            return base.FindAction(controllerContext, controllerDescriptor, actionName);
        }
    }
}