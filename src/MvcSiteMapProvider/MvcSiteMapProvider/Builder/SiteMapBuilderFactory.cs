using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Visitor;
using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Abstract factory that creates instances of <see cref="T:MvcSiteMapProvider.Builder.SiteMapBuilder"/>.
    /// This factory can be used during DI configuration for DI containers that don't support a way to
    /// supply partial lists of constructor parameters. This enables us to create the type without tightly
    /// binding to a specific constructor signature, which makes the DI configuration brittle.
    /// </summary>
    public class SiteMapBuilderFactory
    {
        public SiteMapBuilderFactory(
            ISiteMapNodeVisitor siteMapNodeVisitor,
            ISiteMapHierarchyBuilder siteMapHierarchyBuilder,
            ISiteMapNodeHelperFactory siteMapNodeHelperFactory,
            ICultureContextFactory cultureContextFactory
            )
        {
            SiteMapHierarchyBuilder = siteMapHierarchyBuilder ?? throw new ArgumentNullException(nameof(siteMapHierarchyBuilder));
            SiteMapNodeHelperFactory = siteMapNodeHelperFactory ?? throw new ArgumentNullException(nameof(siteMapNodeHelperFactory));
            SiteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
            CultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        protected readonly ISiteMapHierarchyBuilder SiteMapHierarchyBuilder;
        protected readonly ISiteMapNodeHelperFactory SiteMapNodeHelperFactory;
        protected readonly ISiteMapNodeVisitor SiteMapNodeVisitor;
        protected readonly ICultureContextFactory CultureContextFactory;

        public virtual ISiteMapBuilder Create(ISiteMapNodeProvider siteMapNodeProvider)
        {
            return new SiteMapBuilder(
                siteMapNodeProvider,
                SiteMapNodeVisitor,
                SiteMapHierarchyBuilder,
                SiteMapNodeHelperFactory,
                CultureContextFactory);
        }
    }
}
