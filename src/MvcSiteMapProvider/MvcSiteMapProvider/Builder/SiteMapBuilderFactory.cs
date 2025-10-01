using System;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Visitor;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Abstract factory that creates instances of <see cref="T:MvcSiteMapProvider.Builder.SiteMapBuilder" />.
///     This factory can be used during DI configuration for DI containers that don't support a way to
///     supply partial lists of constructor parameters. This enables us to create the type without tightly
///     binding to a specific constructor signature, which makes the DI configuration brittle.
/// </summary>
public class SiteMapBuilderFactory
{
    private readonly ICultureContextFactory _cultureContextFactory;
    private readonly ISiteMapHierarchyBuilder _siteMapHierarchyBuilder;
    private readonly ISiteMapNodeHelperFactory _siteMapNodeHelperFactory;
    private readonly ISiteMapNodeVisitor _siteMapNodeVisitor;

    public SiteMapBuilderFactory(
        ISiteMapNodeVisitor siteMapNodeVisitor,
        ISiteMapHierarchyBuilder siteMapHierarchyBuilder,
        ISiteMapNodeHelperFactory siteMapNodeHelperFactory,
        ICultureContextFactory cultureContextFactory
    )
    {
        _siteMapHierarchyBuilder = siteMapHierarchyBuilder ??
                                   throw new ArgumentNullException(nameof(siteMapHierarchyBuilder));
        _siteMapNodeHelperFactory = siteMapNodeHelperFactory ??
                                    throw new ArgumentNullException(nameof(siteMapNodeHelperFactory));
        _siteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
        _cultureContextFactory =
            cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
    }

    public virtual ISiteMapBuilder Create(ISiteMapNodeProvider siteMapNodeProvider)
    {
        return new SiteMapBuilder(
            siteMapNodeProvider,
            _siteMapNodeVisitor,
            _siteMapHierarchyBuilder,
            _siteMapNodeHelperFactory,
            _cultureContextFactory);
    }
}
