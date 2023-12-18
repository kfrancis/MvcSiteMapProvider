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
            this.siteMapHierarchyBuilder = siteMapHierarchyBuilder ?? throw new ArgumentNullException(nameof(siteMapHierarchyBuilder));
            this.siteMapNodeHelperFactory = siteMapNodeHelperFactory ?? throw new ArgumentNullException(nameof(siteMapNodeHelperFactory));
            this.siteMapNodeVisitor = siteMapNodeVisitor ?? throw new ArgumentNullException(nameof(siteMapNodeVisitor));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        protected readonly ISiteMapHierarchyBuilder siteMapHierarchyBuilder;
        protected readonly ISiteMapNodeHelperFactory siteMapNodeHelperFactory;
        protected readonly ISiteMapNodeVisitor siteMapNodeVisitor;
        protected readonly ICultureContextFactory cultureContextFactory;

        public virtual ISiteMapBuilder Create(ISiteMapNodeProvider siteMapNodeProvider)
        {
            return new SiteMapBuilder(
                siteMapNodeProvider,
                siteMapNodeVisitor,
                siteMapHierarchyBuilder,
                siteMapNodeHelperFactory,
                cultureContextFactory);
        }
    }
}