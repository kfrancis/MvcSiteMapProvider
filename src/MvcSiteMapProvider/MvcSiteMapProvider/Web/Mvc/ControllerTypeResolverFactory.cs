using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Web.Compilation;
using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Web.Mvc.ControllerTypeResolver"/>
    /// at runtime.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class ControllerTypeResolverFactory
        : IControllerTypeResolverFactory
    {
        public ControllerTypeResolverFactory(
            IEnumerable<string> areaNamespacesToIgnore,
            IControllerBuilder controllerBuilder,
            IBuildManager buildManager
            )
        {
            this.areaNamespacesToIgnore = areaNamespacesToIgnore ?? throw new ArgumentNullException(nameof(areaNamespacesToIgnore));
            this.controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
            this.buildManager = buildManager ?? throw new ArgumentNullException(nameof(buildManager));
        }

        protected readonly IEnumerable<string> areaNamespacesToIgnore;
        protected readonly IControllerBuilder controllerBuilder;
        protected readonly IBuildManager buildManager;

        #region IControllerTypeResolverFactory Members

        public IControllerTypeResolver Create(RouteCollection routes)
        {
            return new ControllerTypeResolver(areaNamespacesToIgnore, routes, controllerBuilder, buildManager);
        }

        #endregion IControllerTypeResolverFactory Members
    }
}