using MvcSiteMapProvider.DI;
using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// A set of services useful for creating SiteMap nodes.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class SiteMapNodeCreator
        : ISiteMapNodeCreator
    {
        protected readonly INodeKeyGenerator nodeKeyGenerator;

        protected readonly ISiteMap siteMap;

        protected readonly ISiteMapNodeFactory siteMapNodeFactory;

        protected readonly ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory;

        public SiteMapNodeCreator(
                                            ISiteMap siteMap,
            ISiteMapNodeFactory siteMapNodeFactory,
            INodeKeyGenerator nodeKeyGenerator,
            ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
        {
            this.siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
            this.siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
            this.nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
            this.siteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ?? throw new ArgumentNullException(nameof(siteMapNodeToParentRelationFactory));
        }

        public ISiteMapNodeToParentRelation CreateDynamicSiteMapNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var node = siteMapNodeFactory.CreateDynamic(siteMap, key, implicitResourceKey);
            return siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public ISiteMapNodeToParentRelation CreateSiteMapNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var node = siteMapNodeFactory.Create(siteMap, key, implicitResourceKey);
            return siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public virtual string GenerateSiteMapNodeKey(string parentKey, string key, string url, string title, string area, string controller, string action, string httpMethod, bool clickable)
        {
            return nodeKeyGenerator.GenerateKey(parentKey, key, url, title, area, controller, action, httpMethod, clickable);
        }
    }
}