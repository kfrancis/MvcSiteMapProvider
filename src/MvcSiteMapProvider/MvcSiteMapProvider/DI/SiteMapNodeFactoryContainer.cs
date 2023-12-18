using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Reflection;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Script.Serialization;
using MvcSiteMapProvider.Web.UrlResolver;
using MvcSiteMapProvider.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace MvcSiteMapProvider.DI
{
    /// <summary>
    /// A specialized dependency injection container for resolving a <see cref="T:MvcSiteMapProvider.SiteMapNodeFactory"/> instance.
    /// </summary>
    public class SiteMapNodeFactoryContainer
    {
        private readonly string absoluteFileName;

        private readonly IAttributeAssemblyProvider assemblyProvider;

        private readonly IDynamicNodeProvider[] dynamicNodeProviders;

        private readonly IJavaScriptSerializer javaScriptSerializer;

        private readonly IJsonToDictionaryDeserializer jsonToDictionaryDeserializer;

        private readonly IMvcContextFactory mvcContextFactory;

        private readonly IMvcSiteMapNodeAttributeDefinitionProvider mvcSiteMapNodeAttributeProvider;

        private readonly IRequestCache requestCache;

        private readonly IReservedAttributeNameProvider reservedAttributeNameProvider;

        private readonly ConfigurationSettings settings;

        private readonly ISiteMapNodeUrlResolver[] siteMapNodeUrlResolvers;

        private readonly ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders;

        private readonly IUrlPath urlPath;

        private readonly XmlDistinctAttributeAggregator xmlAggergator
            = new XmlDistinctAttributeAggregator(new SiteMapXmlNameProvider());

        public SiteMapNodeFactoryContainer(
                                                                                                                            ConfigurationSettings settings,
            IMvcContextFactory mvcContextFactory,
            IUrlPath urlPath,
            IReservedAttributeNameProvider reservedAttributeNameProvider)
        {
            if (settings.EnableSiteMapFile)
            {
                absoluteFileName = HostingEnvironment.MapPath(settings.SiteMapFileName);
            }
            this.settings = settings;
            this.mvcContextFactory = mvcContextFactory;
            requestCache = this.mvcContextFactory.GetRequestCache();
            this.urlPath = urlPath;
            this.reservedAttributeNameProvider = reservedAttributeNameProvider;
            javaScriptSerializer = new JavaScriptSerializerAdapter();
            jsonToDictionaryDeserializer = new JsonToDictionaryDeserializer(javaScriptSerializer, this.mvcContextFactory);
            assemblyProvider = new AttributeAssemblyProvider(settings.IncludeAssembliesForScan, settings.ExcludeAssembliesForScan);
            mvcSiteMapNodeAttributeProvider = new MvcSiteMapNodeAttributeDefinitionProvider();
            dynamicNodeProviders = ResolveDynamicNodeProviders();
            siteMapNodeUrlResolvers = ResolveSiteMapNodeUrlResolvers();
            siteMapNodeVisibilityProviders = ResolveSiteMapNodeVisibilityProviders(settings.DefaultSiteMapNodeVisibiltyProvider);
        }

        public ISiteMapNodeFactory ResolveSiteMapNodeFactory()
        {
            return new SiteMapNodeFactory(
                ResolveSiteMapNodeChildStateFactory(),
                ResolveLocalizationServiceFactory(),
                ResolveSiteMapNodePluginProvider(),
                urlPath,
                mvcContextFactory);
        }

        private IEnumerable<string> GetMvcSiteMapNodeAttributeDynamicNodeProviderNames()
        {
            var result = new List<string>();
            if (settings.ScanAssembliesForSiteMapNodes)
            {
                var assemblies = assemblyProvider.GetAssemblies();
                var definitions = mvcSiteMapNodeAttributeProvider.GetMvcSiteMapNodeAttributeDefinitions(assemblies);
                result.AddRange(definitions
                    .Where(x => !string.IsNullOrEmpty(x.SiteMapNodeAttribute.DynamicNodeProvider))
                    .Select(x => x.SiteMapNodeAttribute.DynamicNodeProvider)
                    );
            }
            return result;
        }

        private IEnumerable<string> GetMvcSiteMapNodeAttributeUrlResolverNames()
        {
            var result = new List<string>();
            if (settings.ScanAssembliesForSiteMapNodes)
            {
                var assemblies = assemblyProvider.GetAssemblies();
                var definitions = mvcSiteMapNodeAttributeProvider.GetMvcSiteMapNodeAttributeDefinitions(assemblies);
                result.AddRange(definitions
                    .Where(x => !string.IsNullOrEmpty(x.SiteMapNodeAttribute.UrlResolver))
                    .Select(x => x.SiteMapNodeAttribute.UrlResolver)
                    );
            }
            return result;
        }

        private IEnumerable<string> GetMvcSiteMapNodeAttributeVisibilityProviderNames()
        {
            var result = new List<string>();
            if (settings.ScanAssembliesForSiteMapNodes)
            {
                var assemblies = assemblyProvider.GetAssemblies();
                var definitions = mvcSiteMapNodeAttributeProvider.GetMvcSiteMapNodeAttributeDefinitions(assemblies);
                result.AddRange(definitions
                    .Where(x => !string.IsNullOrEmpty(x.SiteMapNodeAttribute.VisibilityProvider))
                    .Select(x => x.SiteMapNodeAttribute.VisibilityProvider)
                    );
            }
            return result;
        }

        private IList<string> GetMvcSiteMapNodeXmlDistinctAttributeValues(string attributeName)
        {
            IList<string> result = new List<string>();
            if (settings.EnableSiteMapFile)
            {
                var xmlSource = new FileXmlSource(absoluteFileName);
                result = xmlAggergator.GetAttributeValues(xmlSource, attributeName);
            }
            return result;
        }

        private IDynamicNodeProvider[] ResolveDynamicNodeProviders()
        {
            var instantiator = new PluginInstantiator<IDynamicNodeProvider>();
            var typeNames = GetMvcSiteMapNodeXmlDistinctAttributeValues("dynamicNodeProvider");
            var attributeTypeNames = GetMvcSiteMapNodeAttributeDynamicNodeProviderNames();
            foreach (var typeName in attributeTypeNames)
            {
                if (!typeNames.Contains(typeName))
                {
                    typeNames.Add(typeName);
                }
            }

            var providers = instantiator.GetInstances(typeNames);
            return providers.ToArray();
        }

        private ILocalizationServiceFactory ResolveLocalizationServiceFactory()
        {
            return new LocalizationServiceFactory(
                new ExplicitResourceKeyParser(),
                new StringLocalizer(mvcContextFactory));
        }

        private ISiteMapNodeChildStateFactory ResolveSiteMapNodeChildStateFactory()
        {
            return new SiteMapNodeChildStateFactory(
                new AttributeDictionaryFactory(requestCache, reservedAttributeNameProvider, jsonToDictionaryDeserializer),
                new RouteValueDictionaryFactory(requestCache, reservedAttributeNameProvider, jsonToDictionaryDeserializer));
        }

        private ISiteMapNodePluginProvider ResolveSiteMapNodePluginProvider()
        {
            return new SiteMapNodePluginProvider(
                new DynamicNodeProviderStrategy(dynamicNodeProviders),
                new SiteMapNodeUrlResolverStrategy(siteMapNodeUrlResolvers),
                new SiteMapNodeVisibilityProviderStrategy(siteMapNodeVisibilityProviders, settings.DefaultSiteMapNodeVisibiltyProvider));
        }

        private ISiteMapNodeUrlResolver[] ResolveSiteMapNodeUrlResolvers()
        {
            var instantiator = new PluginInstantiator<ISiteMapNodeUrlResolver>();
            var typeNames = GetMvcSiteMapNodeXmlDistinctAttributeValues("urlResolver");
            var attributeTypeNames = GetMvcSiteMapNodeAttributeUrlResolverNames();
            foreach (var typeName in attributeTypeNames)
            {
                if (!typeNames.Contains(typeName))
                {
                    typeNames.Add(typeName);
                }
            }

            // Add the default provider if it is missing
            var defaultName = typeof(SiteMapNodeUrlResolver).ShortAssemblyQualifiedName();
            if (!typeNames.Contains(defaultName))
            {
                typeNames.Add(defaultName);
            }

            var providers = instantiator.GetInstances(typeNames, new object[] { mvcContextFactory, urlPath });
            return providers.ToArray();
        }

        private ISiteMapNodeVisibilityProvider[] ResolveSiteMapNodeVisibilityProviders(string defaultVisibilityProviderName)
        {
            var instantiator = new PluginInstantiator<ISiteMapNodeVisibilityProvider>();
            var typeNames = GetMvcSiteMapNodeXmlDistinctAttributeValues("visibilityProvider");
            var attributeTypeNames = GetMvcSiteMapNodeAttributeVisibilityProviderNames();
            foreach (var typeName in attributeTypeNames)
            {
                if (!typeNames.Contains(typeName))
                {
                    typeNames.Add(typeName);
                }
            }

            // Fixes #196, default instance not created.
            if (!string.IsNullOrEmpty(defaultVisibilityProviderName) && !typeNames.Contains(defaultVisibilityProviderName))
            {
                typeNames.Add(defaultVisibilityProviderName);
            }
            var providers = instantiator.GetInstances(typeNames);
            return providers.ToArray();
        }
    }
}