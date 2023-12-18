#if !MVC2
using MvcSiteMapProvider.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// An implementation of <see cref="T:System.Web.Mvc.IDependencyResolver"/> that wraps another instance of 
    /// <see cref="T:System.Web.Mvc.IDependencyResolver"/> so they can be used in conjunction with each other.
    /// </summary>
    public class DependencyResolverDecorator
        : IDependencyResolver
    {
        public DependencyResolverDecorator(
            IDependencyResolver dependencyResolver,
            ConfigurationSettings settings
            )
        {
            innerDependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private readonly IDependencyResolver innerDependencyResolver;
        private readonly ConfigurationSettings settings;

        #region IDependencyResolver Members

        public object GetService(Type serviceType)
        {
            if (typeof(XmlSiteMapController).Equals(serviceType))
            {
                var xmlSiteMapResultFactoryContainer = new XmlSiteMapResultFactoryContainer(settings);
                return new XmlSiteMapController(xmlSiteMapResultFactoryContainer.ResolveXmlSiteMapResultFactory());
            }

            return innerDependencyResolver.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (typeof(XmlSiteMapController).Equals(serviceType))
            {
                var xmlSiteMapResultFactoryContainer = new XmlSiteMapResultFactoryContainer(settings);
                return new List<object>() { new XmlSiteMapController(xmlSiteMapResultFactoryContainer.ResolveXmlSiteMapResultFactory()) };
            }

            return innerDependencyResolver.GetServices(serviceType);
        }

        #endregion
    }
}
#endif