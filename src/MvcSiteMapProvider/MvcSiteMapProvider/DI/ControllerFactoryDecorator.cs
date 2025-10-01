using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Web.Mvc;
using System.Web.Routing;
#if !MVC2
using System.Web.SessionState;
#endif

namespace MvcSiteMapProvider.DI;

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
        this.innerControllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    private readonly IControllerFactory innerControllerFactory;
    private readonly ConfigurationSettings settings;


    protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
    {
        var xmlSiteMapResultFactoryContainer = new XmlSiteMapResultFactoryContainer(this.settings);
        return new XmlSiteMapController(xmlSiteMapResultFactoryContainer.ResolveXmlSiteMapResultFactory());
    }

    #region IControllerFactory Members

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
        var controllerType = this.GetControllerType(requestContext, controllerName);

        // Yield control back to the original controller factory if this isn't an
        // internal controller.
        if (!typeof(XmlSiteMapController).Equals(controllerType))
        {
            return this.innerControllerFactory.CreateController(requestContext, controllerName);
        }
        return this.GetControllerInstance(requestContext, controllerType);
    }

#if !MVC2 
    public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
    {
        return this.innerControllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
    }
#endif

    public override void ReleaseController(IController controller)
    {
        this.innerControllerFactory.ReleaseController(controller);
    }

    #endregion
}