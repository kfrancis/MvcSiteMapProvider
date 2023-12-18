using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Web.Mvc;
using System.Web.Routing;

#if !MVC2

using System.Web.SessionState;

#endif

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// An implementation of <see cref="T:System.Web.Mvc.IControllerFactory"/> that wraps another instance of
    /// <see cref="T:System.Web.Mvc.IControllerFactory"/> so they can be used in conjunction with each other.
    /// </summary>
    public class ControllerFactoryDecorator
        : DefaultControllerFactory
    {
        public ControllerFactoryDecorator(
            IControllerFactory controllerFactory,
            ConfigurationSettings settings
            )
        {
            innerControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private readonly IControllerFactory innerControllerFactory;
        private readonly ConfigurationSettings settings;

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            var xmlSiteMapResultFactoryContainer = new XmlSiteMapResultFactoryContainer(settings);
            return new XmlSiteMapController(xmlSiteMapResultFactoryContainer.ResolveXmlSiteMapResultFactory());
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException(nameof(requestContext));
            }
            if (string.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentNullException(nameof(controllerName));
            }
            Type controllerType = GetControllerType(requestContext, controllerName);

            // Yield control back to the original controller factory if this isn't an
            // internal controller.
            return !typeof(XmlSiteMapController).Equals(controllerType)
                ? innerControllerFactory.CreateController(requestContext, controllerName)
                : GetControllerInstance(requestContext, controllerType);
        }

#if !MVC2

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return innerControllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }

#endif

        public override void ReleaseController(IController controller)
        {
            innerControllerFactory.ReleaseController(controller);
        }
    }
}