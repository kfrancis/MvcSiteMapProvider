using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using MvcSiteMapProvider.Reflection;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     ReflectionSiteMapNodeProvider class.
///     Builds a <see cref="T:MvcSiteMapProvider.Builder.ISiteMapNodeToParentRelation" /> list based on a
///     set of attributes within an assembly.
/// </summary>
public class ReflectionSiteMapNodeProvider
    : ISiteMapNodeProvider
{
    private const string SourceName = "MvcSiteMapNodeAttribute";
    private readonly IAttributeAssemblyProviderFactory _attributeAssemblyProviderFactory;
    private readonly IMvcSiteMapNodeAttributeDefinitionProvider _attributeNodeDefinitionProvider;
    private readonly IEnumerable<string> _excludeAssemblies;
    private readonly IEnumerable<string> _includeAssemblies;

    public ReflectionSiteMapNodeProvider(
        IEnumerable<string> includeAssemblies,
        IEnumerable<string> excludeAssemblies,
        IAttributeAssemblyProviderFactory attributeAssemblyProviderFactory,
        IMvcSiteMapNodeAttributeDefinitionProvider attributeNodeDefinitionProvider
    )
    {
        _includeAssemblies = includeAssemblies ?? throw new ArgumentNullException(nameof(includeAssemblies));
        _excludeAssemblies = excludeAssemblies ?? throw new ArgumentNullException(nameof(excludeAssemblies));
        _attributeAssemblyProviderFactory = attributeAssemblyProviderFactory ??
                                            throw new ArgumentNullException(
                                                nameof(attributeAssemblyProviderFactory));
        _attributeNodeDefinitionProvider = attributeNodeDefinitionProvider ??
                                           throw new ArgumentNullException(
                                               nameof(attributeNodeDefinitionProvider));
    }

    public IEnumerable<ISiteMapNodeToParentRelation> GetSiteMapNodes(ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();

        var definitions = GetMvcSiteMapNodeAttributeDefinitions();
        result.AddRange(LoadSiteMapNodesNodesFromMvcSiteMapNodeAttributeDefinitions(definitions, helper));

        // Done!
        return result;
    }

    protected virtual IEnumerable<ISiteMapNodeToParentRelation>
        LoadSiteMapNodesNodesFromMvcSiteMapNodeAttributeDefinitions(
            IEnumerable<IMvcSiteMapNodeAttributeDefinition> definitions, ISiteMapNodeHelper helper)
    {
        var sourceNodes = new List<ISiteMapNodeToParentRelation>();

        sourceNodes.AddRange(CreateNodesFromAttributeDefinitions(definitions, helper));
        sourceNodes.AddRange(CreateDynamicNodes(sourceNodes, helper));

        return sourceNodes;
    }

    protected virtual IEnumerable<IMvcSiteMapNodeAttributeDefinition> GetMvcSiteMapNodeAttributeDefinitions()
    {
        var assemblyProvider = _attributeAssemblyProviderFactory.Create(_includeAssemblies, _excludeAssemblies);
        var assemblies = assemblyProvider.GetAssemblies();
        var definitions = _attributeNodeDefinitionProvider.GetMvcSiteMapNodeAttributeDefinitions(assemblies);
        return definitions;
    }

    protected virtual IList<ISiteMapNodeToParentRelation> CreateNodesFromAttributeDefinitions(
        IEnumerable<IMvcSiteMapNodeAttributeDefinition> definitions, ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();
        foreach (var definition in definitions)
        {
            var node = CreateNodeFromAttributeDefinition(definition, helper);

            // Note: A null node indicates that it doesn't apply to the current siteMapCacheKey
            if (node != null)
            {
                result.Add(node);
            }
        }

        return result;
    }

    protected virtual ISiteMapNodeToParentRelation? CreateNodeFromAttributeDefinition(
        IMvcSiteMapNodeAttributeDefinition definition, ISiteMapNodeHelper helper)
    {
        var result = definition switch
        {
            // Create node
            MvcSiteMapNodeAttributeDefinitionForAction actionNode => GetSiteMapNodeFromMvcSiteMapNodeAttribute(
                actionNode.SiteMapNodeAttribute, actionNode.ControllerType, actionNode.ActionMethodInfo, helper),
            MvcSiteMapNodeAttributeDefinitionForController controllerNode => GetSiteMapNodeFromMvcSiteMapNodeAttribute(
                controllerNode.SiteMapNodeAttribute, controllerNode.ControllerType, null, helper),
            _ => null
        };

        return result;
    }

    protected virtual IList<ISiteMapNodeToParentRelation> CreateDynamicNodes(
        IList<ISiteMapNodeToParentRelation> sourceNodes, ISiteMapNodeHelper helper)
    {
        var result = new List<ISiteMapNodeToParentRelation>();
        foreach (var sourceNode in sourceNodes.Where(x => x.Node.HasDynamicNodeProvider).ToArray())
        {
            result.AddRange(helper.CreateDynamicNodes(sourceNode));

            // Remove the dynamic node from the sources - we are replacing its definition.
            sourceNodes.Remove(sourceNode);
        }

        return result;
    }

    /// <summary>
    ///     Gets the site map node from MVC site map node attribute.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <param name="type">The type.</param>
    /// <param name="methodInfo">The method info.</param>
    /// <param name="helper">The node helper.</param>
    /// <returns></returns>
    protected virtual ISiteMapNodeToParentRelation? GetSiteMapNodeFromMvcSiteMapNodeAttribute(
        IMvcSiteMapNodeAttribute? attribute, Type? type, MethodInfo? methodInfo, ISiteMapNodeHelper helper)
    {
        if (!string.IsNullOrEmpty(attribute?.SiteMapCacheKey))
        {
            // Return null if the attribute doesn't apply to this cache key
            if (helper is { SiteMapCacheKey: not null } && !helper.SiteMapCacheKey.Equals(attribute?.SiteMapCacheKey))
            {
                return null;
            }
        }

        if (methodInfo == null) // try to find Index action
        {
            var ms = type?.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.Equals("Index"));
            if (ms != null)
            {
                foreach (var m in ms)
                {
                    var pars = m.GetParameters();
                    if (pars.Length != 0)
                    {
                        continue;
                    }

                    methodInfo = m;
                    break;
                }
            }
        }

        var area = "";
#pragma warning disable 612,618
        if (!string.IsNullOrEmpty(attribute?.AreaName))
        {
            area = attribute?.AreaName;
        }
#pragma warning restore 612,618
        if (string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(attribute?.Area))
        {
            area = attribute?.Area;
        }

        // Determine area (will only work if controller is defined as [<Anything>.]Areas.<Area>.Controllers.<AnyController>)
        if (string.IsNullOrEmpty(area))
        {
            if (type != null && !string.IsNullOrEmpty(type.Namespace))
            {
                var m = Regex.Match(type.Namespace, @"(?:[^\.]+\.|\s+|^)Areas\.(?<areaName>[^\.]+)\.Controllers");
                if (m.Success)
                {
                    area = m.Groups["areaName"].Value;
                }
            }
        }

        // Determine controller and (index) action
        var controller = type?.Name.Substring(0, type.Name.IndexOf("Controller", StringComparison.OrdinalIgnoreCase));
        var action = methodInfo?.Name ?? "Index";

        if (methodInfo != null)
        {
            // handle ActionNameAttribute
            if (methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true).FirstOrDefault() is
                ActionNameAttribute actionNameAttribute)
            {
                action = actionNameAttribute.Name;
            }
        }

        var httpMethod =
            (string.IsNullOrEmpty(attribute?.HttpMethod) ? nameof(HttpVerbs.Get) : attribute?.HttpMethod)?
            .ToUpper();

        // Handle title
        var title = attribute?.Title;

        // Handle implicit resources
        var implicitResourceKey = attribute?.ResourceKey;

        // Generate key for node
        var key = helper.CreateNodeKey(
            attribute?.ParentKey,
            attribute?.Key,
            attribute?.Url,
            title,
            area,
            controller, action, httpMethod,
            attribute?.Clickable ?? true);

        var nodeParentMap = helper.CreateNode(key, attribute.ParentKey, SourceName, implicitResourceKey);
        var node = nodeParentMap.Node;

        // Assign defaults
        node.Title = title;
        node.Description = attribute.Description;
        node.Attributes.AddRange(attribute.Attributes, false);
        node.Roles.AddRange(attribute.Roles);
        node.Clickable = attribute.Clickable;
        node.VisibilityProvider = attribute.VisibilityProvider;
        node.DynamicNodeProvider = attribute.DynamicNodeProvider;
        node.ImageUrl = attribute.ImageUrl;
        node.ImageUrlProtocol = attribute.ImageUrlProtocol;
        node.ImageUrlHostName = attribute.ImageUrlHostName;
        node.TargetFrame = attribute.TargetFrame;
        node.HttpMethod = httpMethod;
        if (!string.IsNullOrEmpty(attribute.Url))
        {
            node.Url = attribute.Url;
        }

        node.CacheResolvedUrl = attribute.CacheResolvedUrl;
        node.IncludeAmbientValuesInUrl = attribute.IncludeAmbientValuesInUrl;
        node.Protocol = attribute.Protocol;
        node.HostName = attribute.HostName;
        node.CanonicalKey = attribute.CanonicalKey;
        node.CanonicalUrl = attribute.CanonicalUrl;
        node.CanonicalUrlProtocol = attribute.CanonicalUrlProtocol;
        node.CanonicalUrlHostName = attribute.CanonicalUrlHostName;
        node.MetaRobotsValues.AddRange(attribute.MetaRobotsValues);
        node.LastModifiedDate = string.IsNullOrEmpty(attribute.LastModifiedDate)
            ? DateTime.MinValue
            : DateTime.Parse(attribute.LastModifiedDate);
        node.ChangeFrequency = attribute.ChangeFrequency;
        node.UpdatePriority = attribute.UpdatePriority;
        node.Order = attribute.Order;

        // Handle route details
        node.Route = attribute.Route;
        node.RouteValues.AddRange(attribute.Attributes, false);
        node.PreservedRouteParameters.AddRange(attribute.PreservedRouteParameters, [',', ';']);
        node.UrlResolver = attribute.UrlResolver;

        // Specified area, controller and action properties will override any 
        // provided in the attributes collection.
        if (!string.IsNullOrEmpty(area))
        {
            node.RouteValues.Add("area", area);
        }

        if (!string.IsNullOrEmpty(controller))
        {
            node.RouteValues.Add("controller", controller);
        }

        if (!string.IsNullOrEmpty(action))
        {
            node.RouteValues.Add("action", action);
        }

        return nodeParentMap;
    }
}
