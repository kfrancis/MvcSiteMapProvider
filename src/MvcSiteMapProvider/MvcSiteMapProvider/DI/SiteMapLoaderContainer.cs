using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Loader;
using MvcSiteMapProvider.Reflection;
using MvcSiteMapProvider.Visitor;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Xml;
using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// A specialized dependency injection container for resolving a <see cref="T:MvcSiteMapProvider.Loader.SiteMapLoader"/> instance.
    /// </summary>
    internal class SiteMapLoaderContainer
    {
        public SiteMapLoaderContainer(ConfigurationSettings settings)
        {
            // Singleton instances
            if (settings.EnableSiteMapFile)
            {
                absoluteFileName = HostingEnvironment.MapPath(settings.SiteMapFileName);
            }
            mvcContextFactory = new MvcContextFactory();
#if NET35
            this.siteMapCache = new SiteMapCache(new AspNetCacheProvider<ISiteMap>(this.mvcContextFactory));
#else
            siteMapCache = new SiteMapCache(new RuntimeCacheProvider<ISiteMap>(System.Runtime.Caching.MemoryCache.Default));
#endif
            cacheDependency = ResolveCacheDependency(settings);
            requestCache = mvcContextFactory.GetRequestCache();
            bindingFactory = new BindingFactory();
            bindingProvider = new BindingProvider(bindingFactory, mvcContextFactory);
            urlPath = new UrlPath(mvcContextFactory, bindingProvider);
            siteMapCacheKeyGenerator = new SiteMapCacheKeyGenerator(mvcContextFactory);
            siteMapCacheKeyToBuilderSetMapper = new SiteMapCacheKeyToBuilderSetMapper();
            reservedAttributeNameProvider = new ReservedAttributeNameProvider(settings.AttributesToIgnore);
            var siteMapNodeFactoryContainer = new SiteMapNodeFactoryContainer(settings, mvcContextFactory, urlPath, reservedAttributeNameProvider);
            siteMapNodeToParentRelationFactory = new SiteMapNodeToParentRelationFactory();
            nodeKeyGenerator = new NodeKeyGenerator();
            siteMapNodeFactory = siteMapNodeFactoryContainer.ResolveSiteMapNodeFactory();
            siteMapNodeCreatorFactory = ResolveSiteMapNodeCreatorFactory();
            cultureContextFactory = new CultureContextFactory();
            dynamicSiteMapNodeBuilderFactory = new DynamicSiteMapNodeBuilderFactory(siteMapNodeCreatorFactory, cultureContextFactory);
            siteMapHierarchyBuilder = new SiteMapHierarchyBuilder();
            siteMapNodeHelperFactory = ResolveSiteMapNodeHelperFactory();
            siteMapNodeVisitor = ResolveSiteMapNodeVisitor(settings);
            siteMapXmlNameProvider = new SiteMapXmlNameProvider();
            attributeAssemblyProviderFactory = new AttributeAssemblyProviderFactory();
            mvcSiteMapNodeAttributeDefinitionProvider = new MvcSiteMapNodeAttributeDefinitionProvider();
            siteMapNodeProvider = ResolveSiteMapNodeProvider(settings);
            siteMapBuiderSetStrategy = ResolveSiteMapBuilderSetStrategy(settings);
            var siteMapFactoryContainer = new SiteMapFactoryContainer(settings, mvcContextFactory, urlPath);
            siteMapFactory = siteMapFactoryContainer.ResolveSiteMapFactory();
            siteMapCreator = new SiteMapCreator(siteMapCacheKeyToBuilderSetMapper, siteMapBuiderSetStrategy, siteMapFactory);
        }

        private readonly string absoluteFileName;
        private readonly IMvcContextFactory mvcContextFactory;
        private readonly IBindingFactory bindingFactory;
        private readonly IBindingProvider bindingProvider;
        private readonly ISiteMapCache siteMapCache;
        private readonly ICacheDependency cacheDependency;
        private readonly IRequestCache requestCache;
        private readonly IUrlPath urlPath;
        private readonly ISiteMapCacheKeyGenerator siteMapCacheKeyGenerator;
        private readonly ISiteMapCacheKeyToBuilderSetMapper siteMapCacheKeyToBuilderSetMapper;
        private readonly ISiteMapBuilderSetStrategy siteMapBuiderSetStrategy;
        private readonly INodeKeyGenerator nodeKeyGenerator;
        private readonly ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory;
        private readonly ISiteMapNodeFactory siteMapNodeFactory;
        private readonly ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory;
        private readonly ISiteMapNodeHelperFactory siteMapNodeHelperFactory;
        private readonly ISiteMapNodeVisitor siteMapNodeVisitor;
        private readonly ISiteMapNodeProvider siteMapNodeProvider;
        private readonly ISiteMapHierarchyBuilder siteMapHierarchyBuilder;
        private readonly IAttributeAssemblyProviderFactory attributeAssemblyProviderFactory;
        private readonly IMvcSiteMapNodeAttributeDefinitionProvider mvcSiteMapNodeAttributeDefinitionProvider;
        private readonly ICultureContextFactory cultureContextFactory;
        private readonly ISiteMapXmlNameProvider siteMapXmlNameProvider;
        private readonly IReservedAttributeNameProvider reservedAttributeNameProvider;
        private readonly IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory;
        private readonly ISiteMapFactory siteMapFactory;
        private readonly ISiteMapCreator siteMapCreator;

        public ISiteMapLoader ResolveSiteMapLoader()
        {
            return new SiteMapLoader(
                siteMapCache,
                siteMapCacheKeyGenerator,
                siteMapCreator);
        }

        private ISiteMapBuilderSetStrategy ResolveSiteMapBuilderSetStrategy(ConfigurationSettings settings)
        {
            return new SiteMapBuilderSetStrategy(
                new ISiteMapBuilderSet[] {
                    new SiteMapBuilderSet(
                        "default",
                        settings.SecurityTrimmingEnabled,
                        settings.EnableLocalization,
                        settings.VisibilityAffectsDescendants,
                        settings.UseTitleIfDescriptionNotProvided,
                        ResolveSiteMapBuilder(settings),
                        ResolveCacheDetails(settings)
                        )
                    }
                );
        }

        private ISiteMapBuilder ResolveSiteMapBuilder(ConfigurationSettings settings)
        {
            return new SiteMapBuilder(
                siteMapNodeProvider,
                siteMapNodeVisitor,
                siteMapHierarchyBuilder,
                siteMapNodeHelperFactory,
                cultureContextFactory);
        }

        private ISiteMapNodeProvider ResolveSiteMapNodeProvider(ConfigurationSettings settings)
        {
            var providers = new List<ISiteMapNodeProvider>();
            if (settings.EnableSiteMapFile)
            {
                providers.Add(ResolveXmlSiteMapNodeProvider(settings.IncludeRootNodeFromSiteMapFile, settings.EnableSiteMapFileNestedDynamicNodeRecursion));
            }
            if (settings.ScanAssembliesForSiteMapNodes)
            {
                providers.Add(ResolveReflectionSiteMapNodeProvider(settings.IncludeAssembliesForScan, settings.ExcludeAssembliesForScan));
            }
            return new CompositeSiteMapNodeProvider(providers.ToArray());
        }

        private ISiteMapNodeProvider ResolveXmlSiteMapNodeProvider(bool includeRootNode, bool useNestedDynamicNodeRecursion)
        {
            return new XmlSiteMapNodeProvider(
                includeRootNode,
                useNestedDynamicNodeRecursion,
                new FileXmlSource(absoluteFileName),
                siteMapXmlNameProvider);
        }

        private ISiteMapNodeProvider ResolveReflectionSiteMapNodeProvider(IEnumerable<string> includeAssemblies, IEnumerable<string> excludeAssemblies)
        {
            return new ReflectionSiteMapNodeProvider(
                includeAssemblies,
                excludeAssemblies,
                attributeAssemblyProviderFactory,
                mvcSiteMapNodeAttributeDefinitionProvider);
        }

        private ISiteMapNodeVisitor ResolveSiteMapNodeVisitor(ConfigurationSettings settings)
        {
            if (settings.EnableResolvedUrlCaching)
            {
                return new UrlResolvingSiteMapNodeVisitor();
            }
            else
            {
                return new NullSiteMapNodeVisitor();
            }
        }

        private ISiteMapNodeCreatorFactory ResolveSiteMapNodeCreatorFactory()
        {
            return new SiteMapNodeCreatorFactory(
                siteMapNodeFactory,
                nodeKeyGenerator,
                siteMapNodeToParentRelationFactory);
        }

        private ISiteMapNodeHelperFactory ResolveSiteMapNodeHelperFactory()
        {
            return new SiteMapNodeHelperFactory(
                siteMapNodeCreatorFactory,
                dynamicSiteMapNodeBuilderFactory,
                reservedAttributeNameProvider,
                cultureContextFactory);
        }

        private ICacheDetails ResolveCacheDetails(ConfigurationSettings settings)
        {
            return new CacheDetails(
                TimeSpan.FromMinutes(settings.CacheDuration),
                TimeSpan.MinValue,
                cacheDependency
                );
        }

        private ICacheDependency ResolveCacheDependency(ConfigurationSettings settings)
        {
            if (settings.EnableSiteMapFile)
            {
#if NET35
                return new AspNetFileCacheDependency(absoluteFileName);
#else
                return new RuntimeFileCacheDependency(absoluteFileName);
#endif
            }
            else
            {
                return new NullCacheDependency();
            }
        }
    }
}