using System;

namespace MvcSiteMapProvider.Builder
{
    public class SiteMapNodeCreatorFactory
        : ISiteMapNodeCreatorFactory
    {
        protected readonly INodeKeyGenerator NodeKeyGenerator;

        protected readonly ISiteMapNodeFactory SiteMapNodeFactory;

        protected readonly ISiteMapNodeToParentRelationFactory SiteMapNodeToParentRelationFactory;

        public SiteMapNodeCreatorFactory(
                                    ISiteMapNodeFactory siteMapNodeFactory,
            INodeKeyGenerator nodeKeyGenerator,
            ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
        {
            SiteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
            NodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
            SiteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ?? throw new ArgumentNullException(nameof(siteMapNodeToParentRelationFactory));
        }

        public ISiteMapNodeCreator Create(ISiteMap siteMap)
        {
            return new SiteMapNodeCreator(
                siteMap,
                SiteMapNodeFactory,
                NodeKeyGenerator,
                SiteMapNodeToParentRelationFactory);
        }
    }
}
