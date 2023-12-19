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
        protected readonly ICultureContextFactory CultureContextFactory;

        protected readonly IDynamicSiteMapNodeBuilderFactory DynamicSiteMapNodeBuilderFactory;

        protected readonly IReservedAttributeNameProvider ReservedAttributeNameProvider;

        protected readonly ISiteMapNodeCreatorFactory SiteMapNodeCreatorFactory;

        public SiteMapNodeHelperFactory(
                                            ISiteMapNodeCreatorFactory siteMapNodeCreatorFactory,
            IDynamicSiteMapNodeBuilderFactory dynamicSiteMapNodeBuilderFactory,
            IReservedAttributeNameProvider reservedAttributeNameProvider,
            ICultureContextFactory cultureContextFactory
            )
        {
            SiteMapNodeCreatorFactory = siteMapNodeCreatorFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCreatorFactory));
            DynamicSiteMapNodeBuilderFactory = dynamicSiteMapNodeBuilderFactory ?? throw new ArgumentNullException(nameof(dynamicSiteMapNodeBuilderFactory));
            ReservedAttributeNameProvider = reservedAttributeNameProvider ?? throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
            CultureContextFactory = cultureContextFactory ?? throw new ArgumentNullException(nameof(cultureContextFactory));
        }

        public ISiteMapNodeHelper Create(ISiteMap siteMap, ICultureContext cultureContext)
        {
            return new SiteMapNodeHelper(
                siteMap,
                cultureContext,
                SiteMapNodeCreatorFactory,
                DynamicSiteMapNodeBuilderFactory,
                ReservedAttributeNameProvider,
                CultureContextFactory);
        }
    }
}
