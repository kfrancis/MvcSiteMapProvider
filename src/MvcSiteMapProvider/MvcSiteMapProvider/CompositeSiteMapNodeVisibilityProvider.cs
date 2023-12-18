using MvcSiteMapProvider.DI;
using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Chains together a group of ISiteMapNodeVisibilityProvider instances so that visibility logic
    /// for different purposes can be kept in different providers, but still apply to a single node.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class CompositeSiteMapNodeVisibilityProvider
        : ISiteMapNodeVisibilityProvider
    {
        private readonly string instanceName;

        private readonly ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders;

        public CompositeSiteMapNodeVisibilityProvider(string instanceName, params ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new ArgumentNullException(nameof(instanceName));
            this.instanceName = instanceName;
            this.siteMapNodeVisibilityProviders = siteMapNodeVisibilityProviders ?? throw new ArgumentNullException(nameof(siteMapNodeVisibilityProviders));
        }

        public bool AppliesTo(string providerName)
        {
            return instanceName.Equals(providerName, StringComparison.Ordinal);
        }

        public bool IsVisible(ISiteMapNode node, IDictionary<string, object> sourceMetadata)
        {
            // Result is always true unless the first provider that returns false is encountered.
            var result = true;
            foreach (var visibilityProvider in siteMapNodeVisibilityProviders)
            {
                result = visibilityProvider.IsVisible(node, sourceMetadata);
                if (!result)
                    return false;
            }
            return result;
        }
    }
}