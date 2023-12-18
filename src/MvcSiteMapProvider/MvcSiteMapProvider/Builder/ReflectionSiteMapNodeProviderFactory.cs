using MvcSiteMapProvider.Reflection;
using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Abstract factory to assist with the creation of ReflectionSiteMapNodeProvider for DI containers
    /// that don't support injection of a partial list of constructor parameters. Without using this
    /// class, DI configuration code for those containers is very brittle.
    /// </summary>
    public class ReflectionSiteMapNodeProviderFactory
    {
        protected readonly IAttributeAssemblyProviderFactory attributeAssemblyProviderFactory;

        protected readonly IMvcSiteMapNodeAttributeDefinitionProvider attributeNodeDefinitionProvider;

        public ReflectionSiteMapNodeProviderFactory(
                            IAttributeAssemblyProviderFactory attributeAssemblyProviderFactory,
            IMvcSiteMapNodeAttributeDefinitionProvider attributeNodeDefinitionProvider
            )
        {
            this.attributeAssemblyProviderFactory = attributeAssemblyProviderFactory ?? throw new ArgumentNullException(nameof(attributeAssemblyProviderFactory));
            this.attributeNodeDefinitionProvider = attributeNodeDefinitionProvider ?? throw new ArgumentNullException(nameof(attributeNodeDefinitionProvider));
        }

        public ReflectionSiteMapNodeProvider Create(IEnumerable<String> includeAssemblies, IEnumerable<String> excludeAssemblies)
        {
            return new ReflectionSiteMapNodeProvider(
                includeAssemblies,
                excludeAssemblies,
                attributeAssemblyProviderFactory,
                attributeNodeDefinitionProvider);
        }

        public ReflectionSiteMapNodeProvider Create(IEnumerable<String> includeAssemblies)
        {
            return new ReflectionSiteMapNodeProvider(
                includeAssemblies,
                new string[0],
                attributeAssemblyProviderFactory,
                attributeNodeDefinitionProvider);
        }
    }
}