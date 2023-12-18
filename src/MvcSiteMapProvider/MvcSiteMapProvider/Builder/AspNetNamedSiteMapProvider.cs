using System;
using System.Web;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Provider for ASP.NET classic SiteMapProvider. Use this class to
    /// get the provider configured in the sitemap/providers section of
    /// Web.config by name.
    /// </summary>
    public class AspNetNamedSiteMapProvider
        : IAspNetSiteMapProvider
    {
        protected readonly string ProviderName;

        public AspNetNamedSiteMapProvider(
                    string providerName
            )
        {
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException(providerName);
            ProviderName = providerName;
        }

        public SiteMapProvider GetProvider()
        {
            return System.Web.SiteMap.Providers[ProviderName];
        }
    }
}
