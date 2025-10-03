using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Used to chain several <see cref="T:MvcSiteMapProvider.Builder.ISiteMapBuilder" /> instances in succession.
///     The builders will be processed in the same order as they are specified in the constructor.
/// </summary>
public class CompositeSiteMapBuilder
    : ISiteMapBuilder
{
    private readonly IEnumerable<ISiteMapBuilder> _siteMapBuilders;

    public CompositeSiteMapBuilder(params ISiteMapBuilder[] siteMapBuilders)
    {
        _siteMapBuilders = siteMapBuilders ?? throw new ArgumentNullException(nameof(siteMapBuilders));
    }

    public ISiteMapNode? BuildSiteMap(ISiteMap siteMap, ISiteMapNode? rootNode)
    {
        var result = rootNode;
        foreach (var builder in _siteMapBuilders)
        {
            result = builder.BuildSiteMap(siteMap, result);
        }

        return result;
    }
}
