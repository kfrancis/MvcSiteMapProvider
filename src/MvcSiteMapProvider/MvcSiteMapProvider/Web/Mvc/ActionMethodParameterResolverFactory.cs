using System;

namespace MvcSiteMapProvider.Web.Mvc
{
    // TODO: Remove this type in version 5.

    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Web.Mvc.ActionMethodParameterResolver"/>
    /// at runtime.
    /// </summary>
    public class ActionMethodParameterResolverFactory
        : IActionMethodParameterResolverFactory
    {
        protected readonly IControllerDescriptorFactory controllerDescriptorFactory;

        public ActionMethodParameterResolverFactory(
                    IControllerDescriptorFactory controllerDescriptorFactory
            )
        {
            this.controllerDescriptorFactory = controllerDescriptorFactory ?? throw new ArgumentNullException(nameof(controllerDescriptorFactory));
        }

        public IActionMethodParameterResolver Create()
        {
            return new ActionMethodParameterResolver(controllerDescriptorFactory);
        }
    }
}