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
        protected readonly ISiteMapNode node;

        protected readonly string parentKey;

        protected readonly string sourceName;

        public SiteMapNodeToParentRelation(
                                    string parentKey,
            ISiteMapNode node,
            string sourceName
            )
        {
            this.parentKey = parentKey;
            this.node = node ?? throw new ArgumentNullException(nameof(node));
            this.sourceName = sourceName;
        }

        public virtual ISiteMapNode Node
        {
            get { return node; }
        }

        public virtual string ParentKey
        {
            get { return parentKey; }
        }

        public virtual string SourceName
        {
            get { return sourceName; }
        }
    }
}