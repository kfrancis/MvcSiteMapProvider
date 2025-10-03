using System;
using System.Web.Routing;
using System.Web.Mvc;

namespace DI
{
    internal class InjectableControllerFactory
        : DefaultControllerFactory
    {
        private readonly IDependencyInjectionContainer _container;

        public InjectableControllerFactory(IDependencyInjectionContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (requestContext.HttpContext.Request.Url != null && requestContext.HttpContext.Request.Url.ToString().EndsWith("favicon.ico"))
                return null;

            return controllerType == null ?
                base.GetControllerInstance(requestContext, controllerType) :
                _container.GetInstance(controllerType) as IController;
        }

        public override void ReleaseController(IController controller)
        {
            _container.Release(controller);
        }
    }
}
