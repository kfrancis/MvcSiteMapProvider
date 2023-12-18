using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// A set of services useful for building SiteMap nodes, including dynamic nodes.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapNodeHelper
        : ISiteMapNodeHelper
    {
        protected readonly ICultureContext cultureContext;

        protected readonly ICultureContextFactory cultureContextFactory;

        protected readonly IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory;

        protected readonly IReservedAttributeNameProvider reservedAttributeNameProvider;

        protected readonly ISiteMap siteMap;

        protected readonly ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory;

        public SiteMapNodeHelper(
                                                            ISiteMap siteMap,
            ICultureContext cultureContext,
            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory,
            IReservedAttributeNameProvider reservedAttributeNameProvider,
            ICultureContextFactory cultureContextFactory
            )
        {
            this.siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
            this.cultureContext = cultureContext ?? throw new ArgumentNullException(nameof(cultureContext));
            this.siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            this.dynamicSiteMapNodeBuilderFactory = dynamicSiteMapNodeBuilderFactory ?? throw new ArgumentNullException(nameof(dynamicSiteMapNodeBuilderFactory));
            this.reservedAttributeNameProvider = reservedAttributeNameProvider ?? throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public ICultureContext CultureContext
        {
            get { return cultureContext; }
        }

        public IReservedAttributeNameProvider ReservedAttributeNames
        {
            get { return reservedAttributeNameProvider; }
        }

        public string SiteMapCacheKey
        {
            get { return siteMap.CacheKey; }
        }

        public ICultureContext CreateCultureContext(string cultureName, string uiCultureName)
        {
            return cultureContextFactory.Create(cultureName, uiCultureName);
        }

        public ICultureContext CreateCultureContext(CultureInfo culture, CultureInfo uiCulture)
        {
            return cultureContextFactory.Create(culture, uiCulture);
        }

        public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node)
        {
            return CreateDynamicNodes(node, node.ParentKey);
        }

        public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node, string defaultParentKey)
        {
            var dynamicSiteMapNodeBuilder = dynamicSiteMapNodeBuilderFactory.Create(siteMap, CultureContext);
            return dynamicSiteMapNodeBuilder.BuildDynamicNodes(node.Node, defaultParentKey);
        }

        public ICultureContext CreateInvariantCultureContext()
        {
            return cultureContextFactory.CreateInvariant();
        }

        public ISiteMapNodeToParentRelation CreateNode(string key, string parentKey, string sourceName)
        {
            return CreateNode(key, parentKey, sourceName, null);
        }

        public ISiteMapNodeToParentRelation CreateNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var siteMapNodeCreator = siteMapNodeCreatorFactory.Create(siteMap);
            return siteMapNodeCreator.CreateSiteMapNode(key, parentKey, sourceName, implicitResourceKey);
        }

        public virtual string CreateNodeKey(string parentKey, string key, string url, string title, string area, string controller, string action, string httpMethod, bool clickable)
        {
            var siteMapNodeCreator = siteMapNodeCreatorFactory.Create(siteMap);
            return siteMapNodeCreator.GenerateSiteMapNodeKey(parentKey, key, url, title, area, controller, action, httpMethod, clickable);
        }
    }
}