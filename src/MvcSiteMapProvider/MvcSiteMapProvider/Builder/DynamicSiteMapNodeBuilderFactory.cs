using MvcSiteMapProvider.Globalization;
using System;

namespace MvcSiteMapProvider.Builder
{
    public class DynamicSiteMapNodeBuilderFactory
        : IDynamicSiteMapNodeBuilderFactory
    {
        public DynamicSiteMapNodeBuilderFactory(
            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            ICultureContextFactory cultureContextFactory
            )
        {
            this.siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        private readonly ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory;
        private readonly ICultureContextFactory cultureContextFactory;

        #region IDynamicSiteMapNodeBuilderFactory Members

        public IDynamicSiteMapNodeBuilder Create(ISiteMap siteMap, ICultureContext cultureContext)
        {
            var siteMapNodeCreator = this.siteMapNodeCreatorFactory.Create(siteMap);
            return new DynamicSiteMapNodeBuilder(siteMapNodeCreator, cultureContext, this.cultureContextFactory);
        }

        #endregion
    }
}
