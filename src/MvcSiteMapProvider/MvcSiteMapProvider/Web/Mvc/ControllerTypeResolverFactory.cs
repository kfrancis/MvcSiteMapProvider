using System;
using System.Collections.Generic;
using System.Web.Routing;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Web.Compilation;

namespace MvcSiteMapProvider.Web.Mvc;

/// <summary>
///     An abstract factory that can be used to create new instances of
///     <see cref="T:MvcSiteMapProvider.Web.Mvc.ControllerTypeResolver" />
///     at runtime.
/// </summary>
[ExcludeFromAutoRegistration]
public class ControllerTypeResolverFactory
    : IControllerTypeResolverFactory
{
    private readonly IEnumerable<string> _areaNamespacesToIgnore;
    private readonly IBuildManager _buildManager;
    private readonly IControllerBuilder _controllerBuilder;

    public ControllerTypeResolverFactory(
        IEnumerable<string> areaNamespacesToIgnore,
        IControllerBuilder controllerBuilder,
        IBuildManager buildManager
    )
    {
        _areaNamespacesToIgnore =
            areaNamespacesToIgnore ?? throw new ArgumentNullException(nameof(areaNamespacesToIgnore));
        _controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
        _buildManager = buildManager ?? throw new ArgumentNullException(nameof(buildManager));
    }

    public IControllerTypeResolver Create(RouteCollection routes)
    {
        return new ControllerTypeResolver(_areaNamespacesToIgnore, routes, _controllerBuilder, _buildManager);
    }
}
