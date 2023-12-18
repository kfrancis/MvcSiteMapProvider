using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    /// Adaptor class so test doubles can be injected for <see cref="T:System.Web.Mvc.ControllerBuilder"/>.
    /// </summary>
    [Obsolete("Please use the ControllerBuilderAdapter (spelled with an e) instead. This class will be removed in version 5.")]
    public class ControllerBuilderAdaptor
        : IControllerBuilder
    {
        protected readonly ControllerBuilder controllerBuilder;

        public ControllerBuilderAdaptor(
                    ControllerBuilder controllerBuilder
            )
        {
            this.controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
        }

        public HashSet<string> DefaultNamespaces
        {
            get { return controllerBuilder.DefaultNamespaces; }
        }

        public IControllerFactory GetControllerFactory()
        {
            return controllerBuilder.GetControllerFactory();
        }

        public void SetControllerFactory(Type controllerFactoryType)
        {
            controllerBuilder.SetControllerFactory(controllerFactoryType);
        }

        public void SetControllerFactory(IControllerFactory controllerFactory)
        {
            controllerBuilder.SetControllerFactory(controllerFactory);
        }
    }
}