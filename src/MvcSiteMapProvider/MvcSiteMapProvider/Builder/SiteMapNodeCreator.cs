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
        protected readonly ISiteMap siteMap;
        protected readonly ISiteMapNodeFactory siteMapNodeFactory;
        protected readonly INodeKeyGenerator nodeKeyGenerator;
        protected readonly ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory;

        #region ISiteMapNodeService Members

        public ISiteMapNodeToParentRelation CreateSiteMapNode(string key, string? parentKey, string sourceName, string? implicitResourceKey)
        {
            var node = this.siteMapNodeFactory.Create(this.siteMap, key, implicitResourceKey);
            return this.siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public ISiteMapNodeToParentRelation CreateDynamicSiteMapNode(string key, string parentKey, string sourceName, string implicitResourceKey)
        {
            var node = this.siteMapNodeFactory.CreateDynamic(this.siteMap, key, implicitResourceKey);
            return this.siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
        }

        public virtual string GenerateSiteMapNodeKey(string parentKey, string key, string url, string title, string area, string controller, string action, string httpMethod, bool clickable)
        {
            return this.nodeKeyGenerator.GenerateKey(parentKey, key, url, title, area, controller, action, httpMethod, clickable);
        }

        #endregion
    }
}
