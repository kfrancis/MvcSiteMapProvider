using System;
using System.Collections.Generic;
using System.Globalization;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     A set of services useful for building SiteMap nodes, including dynamic nodes.
/// </summary>
[ExcludeFromAutoRegistration]
public class SiteMapNodeHelper
    : ISiteMapNodeHelper
{
    private readonly ICultureContextFactory _cultureContextFactory;
    private readonly IDynamicSiteMapNodeBuilderFactory _dynamicSiteMapNodeBuilderFactory;

    private readonly ISiteMap _siteMap;
    private readonly ISiteMapNodeCreatorFactory _siteMapNodeCreatorFactory;

    public SiteMapNodeHelper(
        ISiteMap siteMap,
        ICultureContext cultureContext,
        ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
        IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory,
        IReservedAttributeNameProvider reservedAttributeNameProvider,
        ICultureContextFactory cultureContextFactory
    )
    {
        _siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
        CultureContext = cultureContext ?? throw new ArgumentNullException(nameof(cultureContext));
        _siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ??
                                     throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
        _dynamicSiteMapNodeBuilderFactory = dynamicSiteMapNodeBuilderFactory ??
                                            throw new ArgumentNullException(
                                                nameof(dynamicSiteMapNodeBuilderFactory));
        ReservedAttributeNames = reservedAttributeNameProvider ??
                                 throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _cultureContextFactory =
            cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
    }

    public virtual string CreateNodeKey(string parentKey, string key, string url, string title, string area,
        string controller, string action, string httpMethod, bool clickable)
    {
        var siteMapNodeCreator = _siteMapNodeCreatorFactory.Create(_siteMap);
        return siteMapNodeCreator.GenerateSiteMapNodeKey(parentKey, key, url, title, area, controller, action,
            httpMethod, clickable);
    }

    public ISiteMapNodeToParentRelation CreateNode(string key, string parentKey, string sourceName)
    {
        return CreateNode(key, parentKey, sourceName, null);
    }

    public ISiteMapNodeToParentRelation CreateNode(string key, string? parentKey, string sourceName,
        string? implicitResourceKey)
    {
        var siteMapNodeCreator = _siteMapNodeCreatorFactory.Create(_siteMap);
        return siteMapNodeCreator.CreateSiteMapNode(key, parentKey, sourceName, implicitResourceKey);
    }

    public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node)
    {
        return CreateDynamicNodes(node, node.ParentKey);
    }

    public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node,
        string? defaultParentKey)
    {
        var dynamicSiteMapNodeBuilder = _dynamicSiteMapNodeBuilderFactory.Create(_siteMap, CultureContext);
        return dynamicSiteMapNodeBuilder.BuildDynamicNodes(node.Node, defaultParentKey);
    }

    public IReservedAttributeNameProvider ReservedAttributeNames { get; }

    public string? SiteMapCacheKey => _siteMap.CacheKey;

    public ICultureContext CultureContext { get; }

    public ICultureContext CreateCultureContext(string cultureName, string uiCultureName)
    {
        return _cultureContextFactory.Create(cultureName, uiCultureName);
    }

    public ICultureContext CreateCultureContext(CultureInfo culture, CultureInfo uiCulture)
    {
        return _cultureContextFactory.Create(culture, uiCulture);
    }

    public ICultureContext CreateInvariantCultureContext()
    {
        return _cultureContextFactory.CreateInvariant();
    }
}
