using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Tracks all of the registered instances of <see cref="T:MvcSiteMapProvider.IDynamicNodeProvider"/> and
    /// allows the caller to get a specific named instance of <see cref="T:MvcSiteMapProvider.IDynamicNodeProvider"/> at runtime.
    /// </summary>
    public class DynamicNodeProviderStrategy
        : IDynamicNodeProviderStrategy
    {
        private readonly IDynamicNodeProvider[] dynamicNodeProviders;

        public DynamicNodeProviderStrategy(IDynamicNodeProvider[] dynamicNodeProviders)
        {
            this.dynamicNodeProviders = dynamicNodeProviders ?? throw new ArgumentNullException(nameof(dynamicNodeProviders));
        }

        public IEnumerable<DynamicNode> GetDynamicNodeCollection(string providerName, ISiteMapNode node)
        {
            var provider = GetProvider(providerName);
            if (provider == null) return new List<DynamicNode>(); // No provider, return empty collection
            return provider.GetDynamicNodeCollection(node);
        }

        public IDynamicNodeProvider GetProvider(string providerName)
        {
            var provider = Array.Find(dynamicNodeProviders, x => x.AppliesTo(providerName));
            return provider == null && !string.IsNullOrEmpty(providerName)
                ? throw new MvcSiteMapException(string.Format(Resources.Messages.NamedDynamicNodeProviderNotFound, providerName))
                : provider;
        }
    }
}