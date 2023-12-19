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
        private readonly ISiteMapNode _node;
        private readonly string _parentKey;
        private readonly string _sourceName;

        public SiteMapNodeToParentRelation(
                                    string parentKey,
            ISiteMapNode node,
            string sourceName
            )
        {
            _parentKey = parentKey;
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _sourceName = sourceName;
        }

        public virtual ISiteMapNode Node
        {
            get { return _node; }
        }

        public virtual string ParentKey
        {
            get { return _parentKey; }
        }

        public virtual string SourceName
        {
            get { return _sourceName; }
        }
    }
}
