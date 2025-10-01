using System;
using System.Collections.Generic;
using System.Linq;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider;

/// <summary>
///     Default implementation of the <see cref="T:MvcSiteMapProvider.ISiteMapNodeVisibilityProviderStrategy" /> contract.
/// </summary>
public class SiteMapNodeVisibilityProviderStrategy
    : ISiteMapNodeVisibilityProviderStrategy
{
    private readonly string defaultProviderName;

    private readonly ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders;

    public SiteMapNodeVisibilityProviderStrategy(ISiteMapNodeVisibilityProvider[] siteMapNodeVisibilityProviders,
        string defaultProviderName)
    {
        this.siteMapNodeVisibilityProviders = siteMapNodeVisibilityProviders ??
                                              throw new ArgumentNullException(
                                                  nameof(siteMapNodeVisibilityProviders));
        this.defaultProviderName = defaultProviderName;
    }

    public ISiteMapNodeVisibilityProvider? GetProvider(string providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            // Get the configured default provider
            providerName = defaultProviderName;
        }

        var provider = siteMapNodeVisibilityProviders.FirstOrDefault(x => x.AppliesTo(providerName));
        if (provider == null && !string.IsNullOrEmpty(providerName))
        {
            throw new MvcSiteMapException(string.Format(Messages.NamedSiteMapNodeVisibilityProviderNotFound,
                providerName));
        }

        return provider;
    }

    public bool IsVisible(string providerName, ISiteMapNode node, IDictionary<string, object?> sourceMetadata)
    {
        var provider = GetProvider(providerName);
        return provider == null || provider.IsVisible(node, sourceMetadata); // If no provider configured, then always visible.
    }
}