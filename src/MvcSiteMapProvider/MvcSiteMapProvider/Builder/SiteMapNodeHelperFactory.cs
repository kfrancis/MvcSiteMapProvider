using MvcSiteMapProvider.Globalization;
using System;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Abstract factory that creates instances of <see cref="T:MvcSiteMapProvider.Builder.ISiteMapNodeHelper"/>.
    /// </summary>
    public class SiteMapNodeHelperFactory
        : ISiteMapNodeHelperFactory
    {
        public SiteMapNodeHelperFactory(
            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory,
            IReservedAttributeNameProvider reservedAttributeNameProvider,
            ICultureContextFactory cultureContextFactory
            )
        {
            this.siteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            this.dynamicSiteMapNodeBuilderFactory = dynamicSiteMapNodeBuilderFactory ?? throw new ArgumentNullException(nameof(dynamicSiteMapNodeBuilderFactory));
            this.reservedAttributeNameProvider = reservedAttributeNameProvider ?? throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
            this.cultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }
        protected readonly ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory;
        protected readonly IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory;
        protected readonly IReservedAttributeNameProvider reservedAttributeNameProvider;
        protected readonly ICultureContextFactory cultureContextFactory;

        #region ISiteMapNodeHelperFactory Members

        public ISiteMapNodeHelper Create(ISiteMap siteMap, ICultureContext cultureContext)
        {
            return new SiteMapNodeHelper(
                siteMap,
                cultureContext,
                this.siteMapNodeCreatorFactory,
                this.dynamicSiteMapNodeBuilderFactory,
                this.reservedAttributeNameProvider,
                this.cultureContextFactory);
        }

        #endregion
    }
}
