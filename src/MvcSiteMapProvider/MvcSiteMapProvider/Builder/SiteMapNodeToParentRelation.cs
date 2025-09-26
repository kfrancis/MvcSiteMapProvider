using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Class for tracking the relationship between node instances to their parent nodes 
    /// before they are added to the SiteMap.
    /// </summary>
    public class SiteMapNodeToParentRelation
        : ISiteMapNodeToParentRelation
    {
        public SiteMapNodeToParentRelation(
            string? parentKey,
            ISiteMapNode node,
            string sourceName
            )
        {
            this.parentKey = parentKey;
            this.node = node ?? throw new ArgumentNullException(nameof(node));
            this.sourceName = sourceName;
        }
        private readonly string? parentKey;
        private readonly ISiteMapNode node;
        private readonly string sourceName;

        public virtual string? ParentKey => this.parentKey;

        public virtual ISiteMapNode Node => this.node;

        public virtual string SourceName => this.sourceName;

    }
}
