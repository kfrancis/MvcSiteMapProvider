using MvcSiteMapProvider.Visitor;
using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Provides a means of optimizing <see cref="T:MvcSiteMapProvider.ISiteMapNode"/> instances before they
    /// are placed in the cahce.
    /// </summary>
    [Obsolete("VisitingSiteMapBuilder has been deprecated and will be removed in version 5. This functionality was merged into the SiteMapBuilder class, as it only makes sense to run this after the sitemap is completely built.")]
    public class VisitingSiteMapBuilder
        : ISiteMapBuilder
    {
        protected readonly ISiteMapNodeVisitor siteMapNodeVisitor;

        public VisitingSiteMapBuilder(
                    ISiteMapNodeVisitor siteMapNodeVisitor
            )
        {
            this.siteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
        }

        public virtual ISiteMapNode BuildSiteMap(ISiteMap siteMap, ISiteMapNode rootNode)
        {
            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode), Resources.Messages.VisitingSiteMapBuilderRequiresRootNode);
            }

            VisitNodes(rootNode);
            return rootNode;
        }

        protected virtual void VisitNodes(ISiteMapNode node)
        {
            siteMapNodeVisitor.Execute(node);

            if (node.HasChildNodes)
            {
                foreach (var childNode in node.ChildNodes)
                {
                    VisitNodes(childNode);
                }
            }
        }
    }
}