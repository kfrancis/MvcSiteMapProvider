using MvcSiteMapProvider.DI;
using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    /// Facade service that resolves MVC dependencies.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class MvcResolver
        : IMvcResolver
    {
        protected readonly IActionMethodParameterResolver actionMethodParameterResolver;

        protected readonly IControllerTypeResolver controllerTypeResolver;

        public MvcResolver(
                            IControllerTypeResolver controllerTypeResolver,
            IActionMethodParameterResolver actionMethodParameterResolver
            )
        {
            this.controllerTypeResolver = controllerTypeResolver ?? throw new ArgumentNullException(nameof(controllerTypeResolver));
            this.actionMethodParameterResolver = actionMethodParameterResolver ?? throw new ArgumentNullException(nameof(actionMethodParameterResolver));
        }

        public IEnumerable<string> ResolveActionMethodParameters(string areaName, string controllerName, string actionMethodName)
        {
            return actionMethodParameterResolver.ResolveActionMethodParameters(controllerTypeResolver, areaName, controllerName, actionMethodName);
        }

        public Type ResolveControllerType(string areaName, string controllerName)
        {
            return controllerTypeResolver.ResolveControllerType(areaName, controllerName);
        }
    }
}