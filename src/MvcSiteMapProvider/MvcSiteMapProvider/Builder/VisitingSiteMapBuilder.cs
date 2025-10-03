using System;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Visitor;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Provides a means of optimizing <see cref="T:MvcSiteMapProvider.ISiteMapNode" /> instances before they
///     are placed in the cahce.
/// </summary>
[Obsolete(
    "VisitingSiteMapBuilder has been deprecated and will be removed in version 5. This functionality was merged into the SiteMapBuilder class, as it only makes sense to run this after the sitemap is completely built.")]
public class VisitingSiteMapBuilder
    : ISiteMapBuilder
{
    private readonly ISiteMapNodeVisitor _siteMapNodeVisitor;

    public VisitingSiteMapBuilder(
        ISiteMapNodeVisitor siteMapNodeVisitor
    )
    {
        _siteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
    }

    public virtual ISiteMapNode? BuildSiteMap(ISiteMap siteMap, ISiteMapNode? rootNode)
    {
        VisitNodes(rootNode);
        return rootNode;
    }

    protected virtual void VisitNodes(ISiteMapNode node)
    {
        _siteMapNodeVisitor.Execute(node);

        if (!node.HasChildNodes)
        {
            return;
        }

        foreach (var childNode in node.ChildNodes)
        {
            VisitNodes(childNode);
        }
    }
}
