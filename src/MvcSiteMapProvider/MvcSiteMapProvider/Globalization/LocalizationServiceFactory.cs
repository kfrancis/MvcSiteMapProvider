using System;

namespace MvcSiteMapProvider.Globalization
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Globalization.LocalizationService"/>
    /// at runtime.
    /// </summary>
    public class LocalizationServiceFactory
        : ILocalizationServiceFactory
    {
        public LocalizationServiceFactory(
            IExplicitResourceKeyParser explicitResourceKeyParser,
            IStringLocalizer stringLocalizer
            )
        {
            this.explicitResourceKeyParser = explicitResourceKeyParser ?? throw new ArgumentNullException(nameof(explicitResourceKeyParser));
            this.stringLocalizer = stringLocalizer ?? throw new ArgumentNullException(nameof(stringLocalizer));
        }

        protected readonly IExplicitResourceKeyParser explicitResourceKeyParser;
        protected readonly IStringLocalizer stringLocalizer;

        #region ILocalizationServiceFactory Members

        public ILocalizationService Create(string implicitResourceKey)
        {
            return new LocalizationService(implicitResourceKey, explicitResourceKeyParser, stringLocalizer);
        }

        #endregion ILocalizationServiceFactory Members
    }
}