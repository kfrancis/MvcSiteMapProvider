using MvcSiteMapProvider.Globalization;
using System;

namespace MvcSiteMapProvider.Builder
{
    public class DynamicSiteMapNodeBuilderFactory
        : IDynamicSiteMapNodeBuilderFactory
    {
        protected readonly ICultureContextFactory CultureContextFactory;

        protected readonly ISiteMapNodeCreatorFactory SiteMapNodeCreatorFactory;

        public DynamicSiteMapNodeBuilderFactory(
                            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            ICultureContextFactory cultureContextFactory
            )
        {
            SiteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            CultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public IDynamicSiteMapNodeBuilder Create(ISiteMap siteMap, ICultureContext cultureContext)
        {
            var siteMapNodeCreator = SiteMapNodeCreatorFactory.Create(siteMap);
            return new DynamicSiteMapNodeBuilder(siteMapNodeCreator, cultureContext, CultureContextFactory);
        }
    }
}
