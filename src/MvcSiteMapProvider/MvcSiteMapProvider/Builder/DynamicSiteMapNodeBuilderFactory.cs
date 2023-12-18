using MvcSiteMapProvider.Globalization;
using System;

namespace MvcSiteMapProvider.Builder
{
    public class DynamicSiteMapNodeBuilderFactory
        : IDynamicSiteMapNodeBuilderFactory
    {
        protected readonly ICultureContextFactory cultureContextFactory;

        protected readonly ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory;

        public DynamicSiteMapNodeBuilderFactory(
                            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            ICultureContextFactory cultureContextFactory
            )
        {
            this.siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public IDynamicSiteMapNodeBuilder Create(ISiteMap siteMap, ICultureContext cultureContext)
        {
            var siteMapNodeCreator = siteMapNodeCreatorFactory.Create(siteMap);
            return new DynamicSiteMapNodeBuilder(siteMapNodeCreator, cultureContext, cultureContextFactory);
        }
    }
}