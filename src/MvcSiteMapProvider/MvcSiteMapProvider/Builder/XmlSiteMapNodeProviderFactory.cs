using System;
using MvcSiteMapProvider.Xml;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     Abstract factory to assist with the creation of XmlSiteMapNodeProviderFactory for DI containers
///     that don't support injection of a partial list of constructor parameters. Without using this
///     class, DI configuration code for those containers is very brittle.
/// </summary>
public class XmlSiteMapNodeProviderFactory
{
    private readonly ISiteMapXmlNameProvider _xmlNameProvider;

    public XmlSiteMapNodeProviderFactory(
        ISiteMapXmlNameProvider xmlNameProvider
    )
    {
        _xmlNameProvider = xmlNameProvider ?? throw new ArgumentNullException(nameof(xmlNameProvider));
    }

    protected virtual XmlSiteMapNodeProvider Create(IXmlSource xmlSource, bool includeRootNode,
        bool useNestedDynamicNodeRecursion)
    {
        return new XmlSiteMapNodeProvider(includeRootNode, useNestedDynamicNodeRecursion, xmlSource, _xmlNameProvider);
    }

    public virtual XmlSiteMapNodeProvider Create(IXmlSource xmlSource, bool includeRootNode)
    {
        return Create(xmlSource, includeRootNode, false);
    }

    public virtual XmlSiteMapNodeProvider Create(IXmlSource xmlSource)
    {
        return Create(xmlSource, true, false);
    }
}
