using System;
using System.Collections.Generic;
using MvcSiteMapProvider.Reflection;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    ///     Abstract factory to assist with the creation of ReflectionSiteMapNodeProvider for DI containers
    ///     that don't support injection of a partial list of constructor parameters. Without using this
    ///     class, DI configuration code for those containers is very brittle.
    /// </summary>
    public class ReflectionSiteMapNodeProviderFactory
    {
        private readonly IAttributeAssemblyProviderFactory _attributeAssemblyProviderFactory;
        private readonly IMvcSiteMapNodeAttributeDefinitionProvider _attributeNodeDefinitionProvider;

        public ReflectionSiteMapNodeProviderFactory(
            IAttributeAssemblyProviderFactory attributeAssemblyProviderFactory,
            IMvcSiteMapNodeAttributeDefinitionProvider attributeNodeDefinitionProvider
        )
        {
            _attributeAssemblyProviderFactory = attributeAssemblyProviderFactory ??
                                                throw new ArgumentNullException(
                                                    nameof(attributeAssemblyProviderFactory));
            _attributeNodeDefinitionProvider = attributeNodeDefinitionProvider ??
                                               throw new ArgumentNullException(nameof(attributeNodeDefinitionProvider));
        }

        public ReflectionSiteMapNodeProvider Create(IEnumerable<string> includeAssemblies,
            IEnumerable<string> excludeAssemblies)
        {
            return new ReflectionSiteMapNodeProvider(
                includeAssemblies,
                excludeAssemblies,
                _attributeAssemblyProviderFactory,
                _attributeNodeDefinitionProvider);
        }

        public ReflectionSiteMapNodeProvider Create(IEnumerable<string> includeAssemblies)
        {
            return new ReflectionSiteMapNodeProvider(
                includeAssemblies, Array.Empty<string>(),
                _attributeAssemblyProviderFactory,
                _attributeNodeDefinitionProvider);
        }
    }
}
