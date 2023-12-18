using System;

namespace MvcSiteMapProvider.Builder
{
    public class SiteMapNodeCreatorFactory
        : ISiteMapNodeCreatorFactory
    {
        protected readonly INodeKeyGenerator nodeKeyGenerator;

        protected readonly ISiteMapNodeFactory siteMapNodeFactory;

        protected readonly ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory;

        public SiteMapNodeCreatorFactory(
                                    ISiteMapNodeFactory siteMapNodeFactory,
            INodeKeyGenerator nodeKeyGenerator,
            ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
        {
            this.siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
            this.nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
            this.siteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ?? throw new ArgumentNullException(nameof(siteMapNodeToParentRelationFactory));
        }

        public ISiteMapNodeCreator Create(ISiteMap siteMap)
        {
            return new SiteMapNodeCreator(
                siteMap,
                siteMapNodeFactory,
                nodeKeyGenerator,
                siteMapNodeToParentRelationFactory);
        }
    }
}