using System;

namespace MvcSiteMapProvider.Builder
{
    public class SiteMapNodeCreatorFactory
        : ISiteMapNodeCreatorFactory
    {
        public SiteMapNodeCreatorFactory(
            ISiteMapNodeFactory siteMapNodeFactory,
            INodeKeyGenerator nodeKeyGenerator,
            ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
        {
            this.siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
            this.nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
            this.siteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ?? throw new ArgumentNullException(nameof(siteMapNodeToParentRelationFactory));
        }
        protected readonly ISiteMapNodeFactory siteMapNodeFactory;
        protected readonly INodeKeyGenerator nodeKeyGenerator;
        protected readonly ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory;

        #region ISiteMapNodeCreatorFactory Members

        public ISiteMapNodeCreator Create(ISiteMap siteMap)
        {
            return new SiteMapNodeCreator(
                siteMap, 
                this.siteMapNodeFactory, 
                this.nodeKeyGenerator, 
                this.siteMapNodeToParentRelationFactory);
        }

        #endregion
    }
}
