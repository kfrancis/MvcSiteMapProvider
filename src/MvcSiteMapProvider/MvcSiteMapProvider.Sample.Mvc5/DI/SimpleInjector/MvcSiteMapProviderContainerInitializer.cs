using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Security;
using MvcSiteMapProvider.Visitor;
using MvcSiteMapProvider.Web.Compilation;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.UrlResolver;
using MvcSiteMapProvider.Xml;
using SimpleInjector;

namespace MvcSiteMapProvider.Sample.Mvc5.DI.SimpleInjector
{
    public static class MvcSiteMapProviderContainerInitializer
    {
        public static void SetUp(Container container)
        {
            var enableLocalization = true;
            var absoluteFileName = HostingEnvironment.MapPath("~/Mvc.sitemap");
            var absoluteCacheExpiration = TimeSpan.FromMinutes(5);
            var visibilityAffectsDescendants = true;
            var useTitleIfDescriptionNotProvided = true;


            var securityTrimmingEnabled = false;
            var includeAssembliesForScan = new[] { "MvcSiteMapProvider.Sample.Mvc5" };


            // Extension to allow resolution of arrays by GetAllInstances (natively based on IEnumerable).
            // source from: https://simpleinjector.codeplex.com/wikipage?title=CollectionRegistrationExtensions
            AllowToResolveArraysAndLists(container);

            var currentAssembly = typeof(MvcSiteMapProviderContainerInitializer).Assembly;
            var siteMapProviderAssembly = typeof(SiteMaps).Assembly;
            var allAssemblies = new[] { currentAssembly, siteMapProviderAssembly };
            var excludeTypes = new Type[]
            {
                // Use this array to add types you wish to explicitly exclude from convention-based  
                // auto-registration. By default all types that either match I[TypeName] = [TypeName] or 
                // I[TypeName] = [TypeName]Adapter will be automatically wired up as long as they don't 
                // have the [ExcludeFromAutoRegistrationAttribute].
                //
                // If you want to override a type that follows the convention, you should add the name 
                // of either the implementation name or the interface that it inherits to this list and 
                // add your manual registration code below. This will prevent duplicate registrations 
                // of the types from occurring. 

                // Example:
                // typeof(SiteMap),
                // typeof(SiteMapNodeVisibilityProviderStrategy)
            };
            var multipleImplementationTypes = new[]
            {
                typeof(ISiteMapNodeUrlResolver), typeof(ISiteMapNodeVisibilityProvider),
                typeof(IDynamicNodeProvider)
            };

            // Matching type name (I[TypeName] = [TypeName]) or matching type name + suffix Adapter (I[TypeName] = [TypeName]Adapter)
            // and not decorated with the [ExcludeFromAutoRegistrationAttribute].
            CommonConventions.RegisterDefaultConventions(
                (interfaceType, implementationType) => container.RegisterSingleton(interfaceType, implementationType),
                new[] { siteMapProviderAssembly },
                allAssemblies,
                excludeTypes,
                string.Empty);

            // Multiple implementations of strategy based extension points (and not decorated with [ExcludeFromAutoRegistrationAttribute]).
            CommonConventions.RegisterAllImplementationsOfInterfaceSingle(
                (interfaceType, implementationTypes) =>
                    container.Collection.Register(interfaceType, implementationTypes),
                multipleImplementationTypes,
                allAssemblies,
                excludeTypes,
                string.Empty);

            container.Register<XmlSiteMapController>();

            // Visibility Providers
            container.Register<ISiteMapNodeVisibilityProviderStrategy>(() =>
                new SiteMapNodeVisibilityProviderStrategy(
                    container.GetAllInstances<ISiteMapNodeVisibilityProvider>().ToArray(), string.Empty));

            // Pass in the global controllerBuilder reference
            container.Register(() => ControllerBuilder.Current);

            container.Register<IControllerTypeResolverFactory>(() =>
                new ControllerTypeResolverFactory(Array.Empty<string>(),
                    container.GetInstance<IControllerBuilder>(),
                    container.GetInstance<IBuildManager>()));

            // Configure Security
            container.Collection.Register<IAclModule>(typeof(AuthorizeAttributeAclModule), typeof(XmlRolesAclModule));
            container.Register<IAclModule>(() =>
                new CompositeAclModule(container.GetAllInstances<IAclModule>().ToArray()));

            // Setup cache


            container.Register<ObjectCache>(() => MemoryCache.Default);
            container.Register(typeof(ICacheProvider<>), typeof(RuntimeCacheProvider<>));
            container.Register<ICacheDependency>(() => new RuntimeFileCacheDependency(absoluteFileName));

            container.Register<ICacheDetails>(() => new CacheDetails(absoluteCacheExpiration, TimeSpan.MinValue,
                container.GetInstance<ICacheDependency>()));

            // Configure the visitors
            container.Register<ISiteMapNodeVisitor, UrlResolvingSiteMapNodeVisitor>();

            // Prepare for the sitemap node providers
            container.Register<IReservedAttributeNameProvider>(() =>
                new ReservedAttributeNameProvider(Array.Empty<string>()));
            container.Register<IXmlSource>(() => new FileXmlSource(absoluteFileName));

            // Register the sitemap node providers
            container.Register(() => container.GetInstance<XmlSiteMapNodeProviderFactory>()
                .Create(container.GetInstance<IXmlSource>()));
            container.Register(() => container.GetInstance<ReflectionSiteMapNodeProviderFactory>()
                .Create(includeAssembliesForScan));

            // Register the sitemap builders
            container.Register(() => container.GetInstance<SiteMapBuilderFactory>()
                .Create(new CompositeSiteMapNodeProvider(container.GetInstance<XmlSiteMapNodeProvider>(),
                    container.GetInstance<ReflectionSiteMapNodeProvider>())));

            container.Collection.Register(
                ResolveISiteMapBuilderSets(container, securityTrimmingEnabled, enableLocalization,
                    visibilityAffectsDescendants, useTitleIfDescriptionNotProvided));
            container.Register<ISiteMapBuilderSetStrategy>(() =>
                new SiteMapBuilderSetStrategy(container.GetAllInstances<ISiteMapBuilderSet>().ToArray()));
        }

        private static IEnumerable<ISiteMapBuilderSet> ResolveISiteMapBuilderSets(
            Container container, bool securityTrimmingEnabled, bool enableLocalization,
            bool visibilityAffectsDescendants, bool useTitleIfDescriptionNotProvided)
        {
            yield return new SiteMapBuilderSet(
                "default",
                securityTrimmingEnabled,
                enableLocalization,
                visibilityAffectsDescendants,
                useTitleIfDescriptionNotProvided,
                container.GetInstance<ISiteMapBuilder>(),
                container.GetInstance<ICacheDetails>());
        }

        private static void AllowToResolveArraysAndLists(Container container)
        {
            container.ResolveUnregisteredType += (sender, e) =>
            {
                var serviceType = e.UnregisteredServiceType;

                if (serviceType.IsArray)
                {
                    RegisterArrayResolver(e, container,
                        serviceType.GetElementType());
                }
                else if (serviceType.IsGenericType &&
                         serviceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    RegisterArrayResolver(e, container,
                        serviceType.GetGenericArguments()[0]);
                }
            };
        }

        private static void RegisterArrayResolver(UnregisteredTypeEventArgs e, Container container, Type elementType)
        {
            var producer = container.GetRegistration(typeof(IEnumerable<>)
                .MakeGenericType(elementType));
            if (producer == null)
            {
                return;
            }

            var enumerableExpression = producer.BuildExpression();
            var arrayMethod = typeof(Enumerable).GetMethod("ToArray")
                ?.MakeGenericMethod(elementType);
            if (arrayMethod == null)
            {
                return;
            }

            var arrayExpression = Expression.Call(arrayMethod, enumerableExpression);
            e.Register(arrayExpression);
        }
    }
}
