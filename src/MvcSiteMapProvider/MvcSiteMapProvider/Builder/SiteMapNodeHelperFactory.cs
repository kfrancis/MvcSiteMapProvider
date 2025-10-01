using System;
using MvcSiteMapProvider.Globalization;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Abstract factory that creates instances of <see cref="T:MvcSiteMapProvider.Builder.ISiteMapNodeHelper" />.
/// </summary>
public class SiteMapNodeHelperFactory
    : ISiteMapNodeHelperFactory
{
    private readonly ICultureContextFactory _cultureContextFactory;
    private readonly IDynamicSiteMapNodeBuilderFactory _dynamicSiteMapNodeBuilderFactory;
    private readonly IReservedAttributeNameProvider _reservedAttributeNameProvider;
    private readonly ISiteMapNodeCreatorFactory _siteMapNodeCreatorFactory;

    public SiteMapNodeHelperFactory(
        ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
        IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory,
        IReservedAttributeNameProvider reservedAttributeNameProvider,
        ICultureContextFactory cultureContextFactory
    )
    {
        _siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ??
                                     throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
        _dynamicSiteMapNodeBuilderFactory = dynamicSiteMapNodeBuilderFactory ??
                                            throw new ArgumentNullException(
                                                nameof(dynamicSiteMapNodeBuilderFactory));
        _reservedAttributeNameProvider = reservedAttributeNameProvider ??
                                         throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _cultureContextFactory =
            cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
    }

    public ISiteMapNodeHelper Create(ISiteMap siteMap, ICultureContext cultureContext)
    {
        return new SiteMapNodeHelper(
            siteMap,
            cultureContext,
            _siteMapNodeCreatorFactory,
            _dynamicSiteMapNodeBuilderFactory,
            _reservedAttributeNameProvider,
            _cultureContextFactory);
    }
}
