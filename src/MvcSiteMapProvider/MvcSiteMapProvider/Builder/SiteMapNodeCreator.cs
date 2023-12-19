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
        protected readonly INodeKeyGenerator NodeKeyGenerator;

        protected readonly ISiteMap SiteMap;

        protected readonly ISiteMapNodeFactory SiteMapNodeFactory;

        protected readonly ISiteMapNodeToParentRelationFactory SiteMapNodeToParentRelationFactory;

        public SiteMapNodeCreator(
                                            ISiteMap siteMap,
            ISiteMapNodeFactory siteMapNodeFactory,
            INodeKeyGenerator nodeKeyGenerator,
            ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
        {
            SiteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
            SiteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
            NodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
            SiteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ?? throw new ArgumentNullException(nameof(siteMapNodeToParentRelationFactory));
        }

        public ISiteMapNodeToParentRelation CreateDynamicSiteMapNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var node = SiteMapNodeFactory.CreateDynamic(SiteMap, key, implicitResourceKey);
            return SiteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public ISiteMapNodeToParentRelation CreateSiteMapNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var node = SiteMapNodeFactory.Create(SiteMap, key, implicitResourceKey);
            return SiteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public virtual string GenerateSiteMapNodeKey(string parentKey, string key, string url, string title, string area, string controller, string action, string httpMethod, bool clickable)
        {
            return NodeKeyGenerator.GenerateKey(parentKey, key, url, title, area, controller, action, httpMethod, clickable);
        }
    }
}
