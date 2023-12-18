using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Tracks all of the registered instances of <see cref="T:MvcSiteMapProvider.ISiteMapNodeVisiblityProvider"/> and
    /// allows the caller to get a specific named instance of <see cref="T:MvcSiteMapProvider.ISiteMapNodeVisiblityProvider"/> at runtime.
    /// </summary>
    public class SiteMapNodeVisibilityProviderStrategy
        : ISiteMapNodeVisibilityProviderStrategy
    {
        private readonly string defaultProviderName;

        private readonly ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders;

        public SiteMapNodeVisibilityProviderStrategy(ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders, string defaultProviderName)
        {
            this.siteMapNodeVisibilityProviders = siteMapNodeVisibilityProviders ?? throw new ArgumentNullException(nameof(siteMapNodeVisibilityProviders));
            this.defaultProviderName = defaultProviderName;
        }

        public ISiteMapNodeVisibilityProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                // Get the configured default provider
                providerName = defaultProviderName;
            }

            var provider = Array.Find(siteMapNodeVisibilityProviders, x => x.AppliesTo(providerName));
            return provider == null && !string.IsNullOrEmpty(providerName)
                ? throw new MvcSiteMapException(string.Format(Resources.Messages.NamedSiteMapNodeVisibilityProviderNotFound, providerName))
                : provider;
        }

        public bool IsVisible(string providerName, ISiteMapNode node, IDictionary<string, object> sourceMetadata)
        {
            var provider = GetProvider(providerName);
            if (provider == null) return true; // If no provider configured, then always visible.
            return provider.IsVisible(node, sourceMetadata);
        }
    }
}