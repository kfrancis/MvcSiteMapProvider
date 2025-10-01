using System;
using MvcSiteMapProvider.DI;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     A set of services useful for creating SiteMap nodes.
/// </summary>
[ExcludeFromAutoRegistration]
public class SiteMapNodeCreator
    : ISiteMapNodeCreator
{
    private readonly INodeKeyGenerator _nodeKeyGenerator;
    private readonly ISiteMap _siteMap;
    private readonly ISiteMapNodeFactory _siteMapNodeFactory;
    private readonly ISiteMapNodeToParentRelationFactory _siteMapNodeToParentRelationFactory;

    public SiteMapNodeCreator(
        ISiteMap siteMap,
        ISiteMapNodeFactory siteMapNodeFactory,
        INodeKeyGenerator nodeKeyGenerator,
        ISiteMapNodeToParentRelationFactory siteMapNodeToParentRelationFactory)
    {
        _siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
        _siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
        _nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
        _siteMapNodeToParentRelationFactory = siteMapNodeToParentRelationFactory ??
                                              throw new ArgumentNullException(
                                                  nameof(siteMapNodeToParentRelationFactory));
    }

    public ISiteMapNodeToParentRelation CreateSiteMapNode(string key, string? parentKey, string sourceName,
        string? implicitResourceKey)
    {
        var node = _siteMapNodeFactory.Create(_siteMap, key, implicitResourceKey);
        return _siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
    }

    public ISiteMapNodeToParentRelation CreateDynamicSiteMapNode(string key, string parentKey, string sourceName,
        string implicitResourceKey)
    {
        var node = _siteMapNodeFactory.CreateDynamic(_siteMap, key, implicitResourceKey);
        return _siteMapNodeToParentRelationFactory.Create(parentKey, node, sourceName);
    }

    public virtual string GenerateSiteMapNodeKey(string parentKey, string key, string url, string title, string area,
        string controller, string action, string httpMethod, bool clickable)
    {
        return _nodeKeyGenerator.GenerateKey(parentKey, key, url, title, area, controller, action, httpMethod,
            clickable);
    }
}
