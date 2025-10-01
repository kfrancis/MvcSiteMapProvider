using System;

namespace MvcSiteMapProvider.Builder;

public class SiteMapNodeCreatorFactory
    : ISiteMapNodeCreatorFactory
{
    private readonly INodeKeyGenerator _nodeKeyGenerator;
    private readonly ISiteMapNodeFactory _siteMapNodeFactory;
    private readonly ISiteMapNodeToParentRelationFactory _siteMapNodeToParentRelationFactory;

    public SiteMapNodeCreatorFactory(
        ISiteMapNodeFactory siteMapNodeFactory,
        INodeKeyGenerator nodeKeyGenerator,
        ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
    {
        _siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
        _nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
        _siteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ??
                                              throw new ArgumentNullException(
                                                  nameof(siteMapNodeToParentRelationFactory));
    }

    public ISiteMapNodeCreator Create(ISiteMap siteMap)
    {
        return new SiteMapNodeCreator(
            siteMap,
            _siteMapNodeFactory,
            _nodeKeyGenerator,
            _siteMapNodeToParentRelationFactory);
    }
}
