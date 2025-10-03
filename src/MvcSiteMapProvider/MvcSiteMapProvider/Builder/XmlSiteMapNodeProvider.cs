using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Xml.Linq;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Xml;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     XmlSiteMapNodeProvider class. Builds a <see cref="T:MvcSiteMapProvider.Builder.ISiteMapNodeToParentRelation" />
///     list based on a
///     <see cref="T:MvcSiteMapProvider.Xml.IXmlSource" /> instance.
/// </summary>
public class XmlSiteMapNodeProvider
    : ISiteMapNodeProvider
{
    private const string SourceName = ".sitemap XML File";
    private readonly bool _includeRootNode;
    private readonly bool _useNestedDynamicNodeRecursion;
    private readonly ISiteMapXmlNameProvider _xmlNameProvider;
    private readonly IXmlSource _xmlSource;

    public XmlSiteMapNodeProvider(
        bool includeRootNode,
        bool useNestedDynamicNodeRecursion,
        IXmlSource xmlSource,
        ISiteMapXmlNameProvider xmlNameProvider
    )
    {
        _includeRootNode = includeRootNode;
        _useNestedDynamicNodeRecursion = useNestedDynamicNodeRecursion;
        _xmlSource = xmlSource ?? throw new ArgumentNullException(nameof(xmlSource));
        _xmlNameProvider = xmlNameProvider ?? throw new ArgumentNullException(nameof(xmlNameProvider));
    }

    public IEnumerable<ISiteMapNodeToParentRelation> GetSiteMapNodes(ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();
        var xml = _xmlSource.GetXml();
        if (xml != null)
        {
            result.AddRange(LoadSiteMapNodesFromXml(xml, helper));
        }
        else
        {
            // Throw exception because XML was not defined
            throw new MvcSiteMapException(string.Format(Messages.XmlSiteMapNodeProviderXmlNotDefined,
                helper.SiteMapCacheKey));
        }

        return result;
    }

    protected virtual IEnumerable<ISiteMapNodeToParentRelation> LoadSiteMapNodesFromXml(XDocument xml,
        ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();
        _xmlNameProvider.FixXmlNamespaces(xml);

        // Get the root mvcSiteMapNode element, and map this to an MvcSiteMapNode
        var rootElement = GetRootElement(xml) ?? throw new MvcSiteMapException(string.Format(Messages.XmlSiteMapNodeProviderRootNodeNotDefined,
                helper.SiteMapCacheKey));

        // Add the root node
        var rootNode = GetRootNode(xml, rootElement, helper);
        if (_includeRootNode)
        {
            result.Add(rootNode);
        }

        // Process our XML, passing in the main root sitemap node and xml element.
        result.AddRange(ProcessXmlNodes(rootNode.Node, rootElement, NodesToProcess.All, helper));

        // Done!
        return result;
    }

    protected virtual XElement? GetRootElement(XDocument xml)
    {
        // Get the root mvcSiteMapNode element, and map this to an MvcSiteMapNode
        return xml.Element(_xmlNameProvider.RootName)?.Element(_xmlNameProvider.NodeName);
    }

    protected virtual ISiteMapNodeToParentRelation GetRootNode(XDocument xml, XElement rootElement,
        ISiteMapNodeHelper helper)
    {
        return GetSiteMapNodeFromXmlElement(rootElement, null, helper);
    }

    /// <summary>
    ///     Recursively processes our XML document, parsing our siteMapNodes and dynamicNode(s).
    /// </summary>
    /// <param name="parentNode">The parent node to process.</param>
    /// <param name="parentElement">The corresponding parent XML element.</param>
    /// <param name="processFlags">Flags to indicate which nodes to process.</param>
    /// <param name="helper">The node helper.</param>
    protected virtual IList<ISiteMapNodeToParentRelation> ProcessXmlNodes(ISiteMapNode parentNode,
        XElement parentElement, NodesToProcess processFlags, ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();
        var processStandardNodes = (processFlags & NodesToProcess.StandardNodes) == NodesToProcess.StandardNodes;
        var processDynamicNodes = (processFlags & NodesToProcess.DynamicNodes) == NodesToProcess.DynamicNodes;

        foreach (var node in parentElement.Elements())
        {
            if (node.Name != _xmlNameProvider.NodeName)
            {
                // If the current node is not one of the known node types throw and exception
                throw new MvcSiteMapException(string.Format(Messages.XmlSiteMapNodeProviderInvalidSiteMapElement,
                    helper.SiteMapCacheKey));
            }

            var child = GetSiteMapNodeFromXmlElement(node, parentNode, helper);

            if (processStandardNodes && !child.Node.HasDynamicNodeProvider)
            {
                result.Add(child);

                // Continue recursively processing the XML file.
                result.AddRange(ProcessXmlNodes(child.Node, node, processFlags, helper));
            }
            else if (processDynamicNodes && child.Node.HasDynamicNodeProvider)
            {
                // We pass in the parent node key as the default parent because the dynamic node (child) is never added to the sitemap.
                var dynamicNodes = helper.CreateDynamicNodes(child, parentNode.Key);

                foreach (var dynamicNode in dynamicNodes)
                {
                    result.Add(dynamicNode);

                    // Recursively add non-dynamic children for every dynamic node
                    result.AddRange(!_useNestedDynamicNodeRecursion
                        ? ProcessXmlNodes(dynamicNode.Node, node, NodesToProcess.StandardNodes, helper)
                        // Recursively process both dynamic nodes and static nodes.
                        // This is to allow V3 recursion behavior for those who depended on it - it is not a feature.
                        : ProcessXmlNodes(dynamicNode.Node, node, NodesToProcess.All, helper));
                }

                // Process the next nested dynamic node provider. We pass in the parent node as the default 
                // parent because the dynamic node definition node (child) is never added to the sitemap.
                result.AddRange(!_useNestedDynamicNodeRecursion
                    ? ProcessXmlNodes(parentNode, node, NodesToProcess.DynamicNodes, helper)
                    // Continue recursively processing the XML file.
                    // Can't figure out why this is here, but this is the way it worked in V3 and if
                    // anyone depends on the broken recursive behavior, they probably also depend on this.
                    : ProcessXmlNodes(child.Node, node, processFlags, helper));
            }
        }

        return result;
    }

    /// <summary>
    ///     Maps an XMLElement from the XML file to an MvcSiteMapNode.
    /// </summary>
    /// <param name="node">The element to map.</param>
    /// <param name="parentNode">The parent ISiteMapNode</param>
    /// <param name="helper">The node helper.</param>
    /// <returns>An MvcSiteMapNode which represents the XMLElement.</returns>
    protected virtual ISiteMapNodeToParentRelation GetSiteMapNodeFromXmlElement(XElement node, ISiteMapNode? parentNode,
        ISiteMapNodeHelper helper)
    {
        // Get data required to generate the node instance

        // Get area and controller from node declaration or the parent node
        var area = InheritAreaIfNotProvided(node, parentNode);
        var controller = InheritControllerIfNotProvided(node, parentNode);
        var action = node.GetAttributeValue("action");
        var url = node.GetAttributeValue("url");
        var explicitKey = node.GetAttributeValue("key");
        var parentKey = parentNode == null ? "" : parentNode.Key;
        var httpMethod = node.GetAttributeValueOrFallback("httpMethod", nameof(HttpVerbs.Get)).ToUpper();
        var clickable = bool.Parse(node.GetAttributeValueOrFallback("clickable", "true"));
        var title = node.GetAttributeValue("title");
        var implicitResourceKey = node.GetAttributeValue("resourceKey");

        // Generate key for node
        var key = helper.CreateNodeKey(
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
        var nodeParentMap = helper.CreateNode(key, parentKey, SourceName, implicitResourceKey);
        var siteMapNode = nodeParentMap.Node;

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
            DateTime.Parse(node.GetAttributeValueOrFallback("lastModifiedDate", DateTime.MinValue.ToString(CultureInfo.CurrentCulture)));
        siteMapNode.Order = int.Parse(node.GetAttributeValueOrFallback("order", "0"));

        // Handle route details
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
            if (node.Attribute(item) != null)
            {
                throw new MvcSiteMapException(
                    string.Format(Messages.SiteMapNodeSameKeyInRouteValueAndInheritedRouteParameter, key, title, item));
            }

            if (parentNode != null && parentNode.RouteValues.TryGetValue(item, out var value))
            {
                siteMapNode.RouteValues.Add(item, value);
            }
        }

        return nodeParentMap;
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

    [Flags]
    protected enum NodesToProcess
    {
        None = 0,
        StandardNodes = 1,
        DynamicNodes = 2,
        All = StandardNodes | DynamicNodes
    }
}
