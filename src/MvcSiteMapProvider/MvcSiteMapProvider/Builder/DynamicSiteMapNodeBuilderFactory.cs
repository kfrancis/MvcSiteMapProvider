using System;
using MvcSiteMapProvider.Globalization;

namespace MvcSiteMapProvider.Builder;

public class DynamicSiteMapNodeBuilderFactory
    : IDynamicSiteMapNodeBuilderFactory
{
    private readonly ICultureContextFactory _cultureContextFactory;

    private readonly ISiteMapNodeCreatorFactory _siteMapNodeCreatorFactory;

    public DynamicSiteMapNodeBuilderFactory(
        ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
        ICultureContextFactory cultureContextFactory
    )
    {
        _siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ??
                                     throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
        _cultureContextFactory =
            cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
    }

    public IDynamicSiteMapNodeBuilder Create(ISiteMap siteMap, ICultureContext cultureContext)
    {
        var siteMapNodeCreator = _siteMapNodeCreatorFactory.Create(siteMap);
        return new DynamicSiteMapNodeBuilder(siteMapNodeCreator, cultureContext, _cultureContextFactory);
    }
}
