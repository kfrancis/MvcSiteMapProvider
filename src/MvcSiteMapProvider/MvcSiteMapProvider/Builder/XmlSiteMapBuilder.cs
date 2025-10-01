using System;
using System.Globalization;
using System.Web.Mvc;
using System.Xml.Linq;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Xml;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     XmlSiteMapBuilder class. Builds a <see cref="T:MvcSiteMapProvider.ISiteMapNode" /> tree based on a
///     <see cref="T:MvcSiteMapProvider.Xml.IXmlSource" /> instance.
/// </summary>
[Obsolete(
    "XmlSiteMapBuilder has been deprecated and will be removed in version 5. Use XmlSiteMapNodeProvider in conjunction with SiteMapBuilder instead.")]
public class XmlSiteMapBuilder
    : ISiteMapBuilder
{
    private readonly IDynamicNodeBuilder _dynamicNodeBuilder;
    private readonly INodeKeyGenerator _nodeKeyGenerator;
    private readonly ISiteMapNodeFactory _siteMapNodeFactory;
    private readonly ISiteMapXmlNameProvider _xmlNameProvider;

    private readonly IXmlSource _xmlSource;
    protected readonly ISiteMapXmlReservedAttributeNameProvider ReservedAttributeNameProvider;

    public XmlSiteMapBuilder(
        IXmlSource xmlSource,
        ISiteMapXmlReservedAttributeNameProvider reservedAttributeNameProvider,
        INodeKeyGenerator nodeKeyGenerator,
        IDynamicNodeBuilder dynamicNodeBuilder,
        ISiteMapNodeFactory siteMapNodeFactory,
        ISiteMapXmlNameProvider xmlNameProvider
    )
    {
        _xmlSource = xmlSource ?? throw new ArgumentNullException(nameof(xmlSource));
        ReservedAttributeNameProvider = reservedAttributeNameProvider ??
                                        throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
        _dynamicNodeBuilder = dynamicNodeBuilder ?? throw new ArgumentNullException(nameof(dynamicNodeBuilder));
        _siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
        _xmlNameProvider = xmlNameProvider ?? throw new ArgumentNullException(nameof(xmlNameProvider));
    }

    public virtual ISiteMapNode? BuildSiteMap(ISiteMap siteMap, ISiteMapNode? rootNode)
    {
        var xml = _xmlSource.GetXml();
        if (xml != null)
        {
            rootNode = LoadSiteMapFromXml(siteMap, xml);
        }

        // Done!
        return rootNode;
    }

    protected virtual ISiteMapNode LoadSiteMapFromXml(ISiteMap siteMap, XDocument xml)
    {
        _xmlNameProvider.FixXmlNamespaces(xml);

        // Get the root mvcSiteMapNode element, and map this to an MvcSiteMapNode
        var rootElement = GetRootElement(xml);
        var root = GetRootNode(siteMap, xml, rootElement);

        // Fixes #192 root node not added to sitemap
        if (siteMap.FindSiteMapNodeFromKey(root.Key) == null)
        {
            // Add the root node to the sitemap
            siteMap.AddNode(root);
        }

        // Process our XML, passing in the main root sitemap node and XML element.
        ProcessXmlNodes(siteMap, root, rootElement);

        // Done!
        return root;
    }

    protected virtual XElement? GetRootElement(XDocument xml)
    {
        // Get the root mvcSiteMapNode element, and map this to an MvcSiteMapNode
        return xml.Element(_xmlNameProvider.RootName)?.Element(_xmlNameProvider.NodeName);
    }

    protected virtual ISiteMapNode GetRootNode(ISiteMap siteMap, XDocument xml, XElement rootElement)
    {
        return GetSiteMapNodeFromXmlElement(siteMap, rootElement, null);
    }


    /// <summary>
    ///     Maps an XMLElement from the XML file to an MvcSiteMapNode.
    /// </summary>
    /// <param name="siteMap">
    ///     The main sitemap instance to which nodes will be added.
    /// </param>
    /// <param name="node">The element to map.</param>
    /// <param name="parentNode">The parent SiteMapNode</param>
    /// <returns>An MvcSiteMapNode which represents the XMLElement.</returns>
    protected virtual ISiteMapNode GetSiteMapNodeFromXmlElement(ISiteMap siteMap, XElement node,
        ISiteMapNode? parentNode)
    {
        // Get data required to generate the node instance

        // Get area and controller from node declaration or the parent node
        var area = InheritAreaIfNotProvided(node, parentNode);
        var controller = InheritControllerIfNotProvided(node, parentNode);
        var action = node.GetAttributeValue("action");
        var url = node.GetAttributeValue("url");
        var explicitKey = node.GetAttributeValue("key");
        var parentKey = parentNode == null ? "" : parentNode.Key;
        var httpMethod = node.GetAttributeValueOrFallback("httpMethod", nameof(HttpVerbs.Get)).ToUpperInvariant();
        var clickable = bool.Parse(node.GetAttributeValueOrFallback("clickable", "true"));
        var title = node.GetAttributeValue("title");
        var implicitResourceKey = node.GetAttributeValue("resourceKey");

        // Generate key for node
        var key = _nodeKeyGenerator.GenerateKey(
            parentKey,
            explicitKey,
            url,
            title,
            area,
            controller,
            action,
            httpMethod,
            clickable);

        // Create node
        var siteMapNode = _siteMapNodeFactory.Create(siteMap, key, implicitResourceKey);

        // Assign values
        siteMapNode.Title = title;
        siteMapNode.Description = node.GetAttributeValue("description");
        siteMapNode.Attributes.AddRange(node, false);
        siteMapNode.Roles.AddRange(node.GetAttributeValue("roles"), [',', ';']);
        siteMapNode.Clickable = clickable;
        siteMapNode.VisibilityProvider = node.GetAttributeValue("visibilityProvider");
        siteMapNode.DynamicNodeProvider = node.GetAttributeValue("dynamicNodeProvider");
        siteMapNode.ImageUrl = node.GetAttributeValue("imageUrl");
        siteMapNode.ImageUrlProtocol = node.GetAttributeValue("imageUrlProtocol");
        siteMapNode.ImageUrlHostName = node.GetAttributeValue("imageUrlHostName");
        siteMapNode.TargetFrame = node.GetAttributeValue("targetFrame");
        siteMapNode.HttpMethod = httpMethod;
        siteMapNode.Url = url;
        siteMapNode.CacheResolvedUrl = bool.Parse(node.GetAttributeValueOrFallback("cacheResolvedUrl", "true"));
        siteMapNode.IncludeAmbientValuesInUrl =
            bool.Parse(node.GetAttributeValueOrFallback("includeAmbientValuesInUrl", "false"));
        siteMapNode.Protocol = node.GetAttributeValue("protocol");
        siteMapNode.HostName = node.GetAttributeValue("hostName");
        siteMapNode.CanonicalKey = node.GetAttributeValue("canonicalKey");
        siteMapNode.CanonicalUrl = node.GetAttributeValue("canonicalUrl");
        siteMapNode.CanonicalUrlProtocol = node.GetAttributeValue("canonicalUrlProtocol");
        siteMapNode.CanonicalUrlHostName = node.GetAttributeValue("canonicalUrlHostName");
        siteMapNode.MetaRobotsValues.AddRange(node.GetAttributeValue("metaRobotsValues"), [' ']);
        siteMapNode.ChangeFrequency = (ChangeFrequency)Enum.Parse(typeof(ChangeFrequency),
            node.GetAttributeValueOrFallback("changeFrequency", "Undefined"));
        siteMapNode.UpdatePriority = (UpdatePriority)Enum.Parse(typeof(UpdatePriority),
            node.GetAttributeValueOrFallback("updatePriority", "Undefined"));
        siteMapNode.LastModifiedDate =
            DateTime.Parse(node.GetAttributeValueOrFallback("lastModifiedDate",
                DateTime.MinValue.ToString(CultureInfo.CurrentCulture)));
        siteMapNode.Order = int.Parse(node.GetAttributeValueOrFallback("order", "0"));

        // Handle route details

        // Assign to node
        siteMapNode.Route = node.GetAttributeValue("route");
        siteMapNode.RouteValues.AddRange(node, false);
        siteMapNode.PreservedRouteParameters.AddRange(node.GetAttributeValue("preservedRouteParameters"), [
            ',', ';'
        ]);
        siteMapNode.UrlResolver = node.GetAttributeValue("urlResolver");

        // Area and controller may need inheriting from the parent node, so set (or reset) them explicitly
        siteMapNode.Area = area;
        siteMapNode.Controller = controller;
        siteMapNode.Action = action;

        // Add inherited route values to sitemap node
        foreach (var inheritedRouteParameter in node.GetAttributeValue("inheritedRouteParameters")
                     .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            var item = inheritedRouteParameter.Trim();
            if (parentNode != null && parentNode.RouteValues.TryGetValue(item, out var value))
            {
                siteMapNode.RouteValues.Add(item, value);
            }
        }

        return siteMapNode;
    }

    /// <summary>
    ///     Inherits the area from the parent node if it is not provided in the current siteMapNode XML element and the parent
    ///     node is not null.
    /// </summary>
    /// <param name="node">The siteMapNode element.</param>
    /// <param name="parentNode">The parent node.</param>
    /// <returns>The value provided by either the siteMapNode or parentNode.Area.</returns>
    protected virtual string InheritAreaIfNotProvided(XElement node, ISiteMapNode? parentNode)
    {
        var result = node.GetAttributeValue("area");
        if (node.Attribute("area") == null && parentNode != null)
        {
            result = parentNode.Area;
        }

        return result;
    }

    /// <summary>
    ///     Inherits the controller from the parent node if it is not provided in the current siteMapNode XML element and the
    ///     parent node is not null.
    /// </summary>
    /// <param name="node">The siteMapNode element.</param>
    /// <param name="parentNode">The parent node.</param>
    /// <returns>The value provided by either the siteMapNode or parentNode.Controller.</returns>
    protected virtual string InheritControllerIfNotProvided(XElement node, ISiteMapNode? parentNode)
    {
        var result = node.GetAttributeValue("controller");
        if (node.Attribute("controller") == null && parentNode != null)
        {
            result = parentNode.Controller;
        }

        return result;
    }

    /// <summary>
    ///     Recursively processes our XML document, parsing our siteMapNodes and dynamicNode(s).
    /// </summary>
    /// <param name="siteMap">
    ///     The main sitemap instance to which nodes will be added.
    /// </param>
    /// <param name="rootNode">The main root sitemap node.</param>
    /// <param name="rootElement">The main root XML element.</param>
    protected virtual void ProcessXmlNodes(ISiteMap siteMap, ISiteMapNode rootNode, XElement rootElement)
    {
        // Loop through each element below the current root element.
        foreach (var node in rootElement.Elements())
        {
            ISiteMapNode childNode;
            if (node.Name == _xmlNameProvider.NodeName)
            {
                // If this is a normal mvcSiteMapNode then map the xml element
                // to an MvcSiteMapNode, and add the node to the current root.
                childNode = GetSiteMapNodeFromXmlElement(siteMap, node, rootNode);
                var parentNode = rootNode;

                if (!childNode.HasDynamicNodeProvider)
                {
                    siteMap.AddNode(childNode, parentNode);
                }
                else
                {
                    var dynamicNodesCreated = _dynamicNodeBuilder.BuildDynamicNodesFor(siteMap, childNode, parentNode);

                    // Add non-dynamic children for every dynamic node
                    foreach (var dynamicNodeCreated in dynamicNodesCreated)
                    {
                        ProcessXmlNodes(siteMap, dynamicNodeCreated, node);
                    }
                }
            }
            else
            {
                // If the current node is not one of the known node types throw and exception
                throw new Exception(Messages.InvalidSiteMapElement);
            }

            // Continue recursively processing the XML file.
            ProcessXmlNodes(siteMap, childNode, node);
        }
    }
}
