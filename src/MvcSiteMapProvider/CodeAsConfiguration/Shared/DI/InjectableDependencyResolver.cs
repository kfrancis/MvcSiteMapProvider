#if !MVC2
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DI
{
    internal class InjectableDependencyResolver
        : IDependencyResolver
    {
        private readonly IDependencyInjectionContainer _container;
        private readonly IDependencyResolver _dependencyResolver;

        public InjectableDependencyResolver(IDependencyInjectionContainer container,
            IDependencyResolver currentDependencyResolver)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _dependencyResolver = currentDependencyResolver ??
                                  throw new ArgumentNullException(nameof(currentDependencyResolver));
        }

        public object GetService(Type serviceType)
        {
            var result = _container.TryGetInstance(serviceType) ?? _dependencyResolver.GetService(serviceType);
            return result;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }
    }
}
#endif
