using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Xml;

namespace MvcSiteMapProvider.Builder;

/// <summary>
///     ReflectionSiteMapBuilder class (copied from ReflectionSiteMapSource class).
///     Builds a <see cref="T:MvcSiteMapProvider.ISiteMapNode" /> tree based on a
///     set of attributes within an assembly.
/// </summary>
[Obsolete(
    "ReflectionSiteMapBuilder has been deprecated and will be removed in version 5. Use ReflectionSiteMapNodeProvider in conjunction with SiteMapBuilder instead.")]
public class ReflectionSiteMapBuilder
    : ISiteMapBuilder
{
    private readonly IDynamicNodeBuilder _dynamicNodeBuilder;
    private readonly IEnumerable<string> _excludeAssemblies;
    private readonly IEnumerable<string> _includeAssemblies;
    private readonly INodeKeyGenerator _nodeKeyGenerator;
    protected readonly ISiteMapXmlReservedAttributeNameProvider ReservedAttributeNameProvider;
    private readonly ISiteMapCacheKeyGenerator _siteMapCacheKeyGenerator;
    private readonly ISiteMapNodeFactory _siteMapNodeFactory;

    private string? _siteMapCacheKey;

    public ReflectionSiteMapBuilder(
        IEnumerable<string> includeAssemblies,
        IEnumerable<string> excludeAssemblies,
        ISiteMapXmlReservedAttributeNameProvider reservedAttributeNameProvider,
        INodeKeyGenerator nodeKeyGenerator,
        IDynamicNodeBuilder dynamicNodeBuilder,
        ISiteMapNodeFactory siteMapNodeFactory,
        ISiteMapCacheKeyGenerator siteMapCacheKeyGenerator
    )
    {
        _includeAssemblies = includeAssemblies ?? throw new ArgumentNullException(nameof(includeAssemblies));
        _excludeAssemblies = excludeAssemblies ?? throw new ArgumentNullException(nameof(excludeAssemblies));
        ReservedAttributeNameProvider = reservedAttributeNameProvider ??
                                             throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _nodeKeyGenerator = nodeKeyGenerator ?? throw new ArgumentNullException(nameof(nodeKeyGenerator));
        _dynamicNodeBuilder = dynamicNodeBuilder ?? throw new ArgumentNullException(nameof(dynamicNodeBuilder));
        _siteMapNodeFactory = siteMapNodeFactory ?? throw new ArgumentNullException(nameof(siteMapNodeFactory));
        _siteMapCacheKeyGenerator = siteMapCacheKeyGenerator ??
                                        throw new ArgumentNullException(nameof(siteMapCacheKeyGenerator));
    }

    /// <summary>
    ///     Gets the cache key for the current request and caches it, since this class should only be called 1 time per
    ///     request.
    /// </summary>
    /// <remarks>
    ///     Fixes #158 - this key should not be generated in the constructor because HttpContext cannot be accessed
    ///     that early in the application life-cycle when run in IIS Integrated mode.
    /// </remarks>
    protected virtual string? SiteMapCacheKey
    {
        get
        {
            if (string.IsNullOrEmpty(_siteMapCacheKey))
            {
                _siteMapCacheKey = _siteMapCacheKeyGenerator.GenerateKey();
            }

            return _siteMapCacheKey;
        }
    }


    /// <summary>
    ///     Provides the base data on which the context-aware provider can generate a full tree.
    /// </summary>
    /// <param name="siteMap">The siteMap object to populate with the data.</param>
    /// <param name="rootNode">
    ///     The root node of the site map. If null, the builder will attempt to find a root node
    /// </param>
    /// <returns></returns>
    public virtual ISiteMapNode? BuildSiteMap(ISiteMap siteMap, ISiteMapNode? rootNode)
    {
        // List of assemblies
        IEnumerable<Assembly> assemblies;
        if (_includeAssemblies.Any())
        {
            // An include list is given
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => _includeAssemblies.Contains(new AssemblyName(a.FullName).Name));
        }
        else
        {
            // An exclude list is given
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.StartsWith("mscorlib") &&
                            !a.FullName.StartsWith("System") &&
                            !a.FullName.StartsWith("Microsoft") &&
                            !a.FullName.StartsWith("WebDev") &&
                            !a.FullName.StartsWith("SMDiagnostics") &&
                            !a.FullName.StartsWith("Anonymously") &&
                            !a.FullName.StartsWith("App_") &&
                            !_excludeAssemblies.Contains(new AssemblyName(a.FullName).Name));
        }

        foreach (var assembly in assemblies)
        {
            // http://stackoverflow.com/questions/1423733/how-to-tell-if-a-net-assembly-is-dynamic
            if (assembly.ManifestModule is not ModuleBuilder &&
                assembly.ManifestModule.GetType().Namespace != "System.Reflection.Emit")
            {
                rootNode = ProcessNodesInAssembly(siteMap, assembly, rootNode);
            }
        }

        // Done!
        return rootNode;
    }

    /// <summary>
    ///     Processes the nodes in assembly.
    /// </summary>
    /// <param name="siteMap">
    ///     The site map.
    /// </param>
    /// <param name="assembly">The assembly.</param>
    /// <param name="parentNode"></param>
    protected virtual ISiteMapNode? ProcessNodesInAssembly(ISiteMap siteMap, Assembly assembly,
        ISiteMapNode? parentNode)
    {
        // Create a list of all nodes defined in the assembly
        var assemblyNodes = new List<IMvcSiteMapNodeAttributeDefinition>();

        // Retrieve types
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types;
        }

        // Add all types
        foreach (var type in types)
        {
            if (type.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), true) is not IMvcSiteMapNodeAttribute[]
                attributes)
            {
                continue;
            }

            foreach (var attribute in attributes)
            {
                assemblyNodes.Add(new MvcSiteMapNodeAttributeDefinitionForController
                {
                    SiteMapNodeAttribute = attribute,
                    ControllerType = type
                });
            }

            // Add their methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), true).Any());

            foreach (var method in methods)
            {
                attributes =
                    method.GetCustomAttributes(typeof(IMvcSiteMapNodeAttribute), false) as
                        IMvcSiteMapNodeAttribute[] ??
                    Array.Empty<IMvcSiteMapNodeAttribute>();

                foreach (var attribute in attributes)
                {
                    assemblyNodes.Add(new MvcSiteMapNodeAttributeDefinitionForAction
                    {
                        SiteMapNodeAttribute = attribute,
                        ControllerType = type,
                        ActionMethodInfo = method
                    });
                }
            }
        }

        // Create nodes from MVC site map node attribute definitions
        return CreateNodesFromMvcSiteMapNodeAttributeDefinitions(siteMap, parentNode,
            assemblyNodes.OrderBy(n => n.SiteMapNodeAttribute?.Order));
    }

    /// <summary>
    ///     Creates the nodes from MVC site map node attribute definitions.
    /// </summary>
    /// <param name="siteMap"></param>
    /// <param name="parentNode"></param>
    /// <param name="definitions">The definitions.</param>
    protected virtual ISiteMapNode? CreateNodesFromMvcSiteMapNodeAttributeDefinitions(ISiteMap siteMap,
        ISiteMapNode? parentNode, IEnumerable<IMvcSiteMapNodeAttributeDefinition> definitions)
    {
        // A dictionary of nodes to process later (node, parentKey)
        var nodesToProcessLater = new Dictionary<ISiteMapNode, string?>();
        var emptyParentKeyCount = definitions.Count(t => string.IsNullOrEmpty(t.SiteMapNodeAttribute?.ParentKey));

        // Throw a sensible exception if the configuration has more than 1 empty parent key (#179).
        if (emptyParentKeyCount > 1)
        {
            throw new MvcSiteMapException(Messages.ReflectionSiteMapBuilderRootKeyAmbiguous);
        }

        // Find root node
        if (parentNode == null)
        {
            if (emptyParentKeyCount == 1)
            {
                ISiteMapNode? attributedRootNode = null;

                var item = definitions.Single(t => string.IsNullOrEmpty(t.SiteMapNodeAttribute?.ParentKey));

                attributedRootNode = item switch
                {
                    MvcSiteMapNodeAttributeDefinitionForAction actionNode =>
                        // Create node for action
                        GetSiteMapNodeFromMvcSiteMapNodeAttribute(siteMap, actionNode.SiteMapNodeAttribute,
                            actionNode.ControllerType, actionNode.ActionMethodInfo),
                    MvcSiteMapNodeAttributeDefinitionForController controllerNode =>
                        // Create node for controller
                        GetSiteMapNodeFromMvcSiteMapNodeAttribute(siteMap, controllerNode.SiteMapNodeAttribute,
                            controllerNode.ControllerType, null),
                    _ => attributedRootNode
                };

                attributedRootNode?.Attributes.Remove("parentKey");
                parentNode = attributedRootNode;
            }
        }

        // Fixes #192 root node not added to sitemap
        if (siteMap.FindSiteMapNodeFromKey(parentNode?.Key) == null)
        {
            // Add the root node to the sitemap
            siteMap.AddNode(parentNode);
        }

        // Create nodes
        foreach (var assemblyNode in definitions.Where(t => !string.IsNullOrEmpty(t.SiteMapNodeAttribute?.ParentKey)))
        {
            var nodeToAdd = assemblyNode switch
            {
                // Create node
                MvcSiteMapNodeAttributeDefinitionForAction actionNode =>
                    // Create node for action
                    GetSiteMapNodeFromMvcSiteMapNodeAttribute(siteMap, actionNode.SiteMapNodeAttribute,
                        actionNode.ControllerType, actionNode.ActionMethodInfo),
                MvcSiteMapNodeAttributeDefinitionForController controllerNode =>
                    // Create node for controller
                    GetSiteMapNodeFromMvcSiteMapNodeAttribute(siteMap, controllerNode.SiteMapNodeAttribute,
                        controllerNode.ControllerType, null),
                _ => null
            };

            // Add node
            if (nodeToAdd == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(assemblyNode.SiteMapNodeAttribute?.ParentKey))
            {
                throw new MvcSiteMapException(string.Format(Messages.NoParentKeyDefined, nodeToAdd.Controller,
                    nodeToAdd.Action));
            }

            var parentForNode = parentNode != null
                ? siteMap.FindSiteMapNodeFromKey(assemblyNode.SiteMapNodeAttribute?.ParentKey)
                : null;

            if (parentForNode != null)
            {
                if (nodeToAdd.HasDynamicNodeProvider)
                {
                    var dynamicNodesForChildNode =
                        _dynamicNodeBuilder.BuildDynamicNodesFor(siteMap, nodeToAdd, parentForNode);
                    foreach (var dynamicNode in dynamicNodesForChildNode)
                    {
                        // Verify parent/child relation
                        if (parentNode != null &&
                            dynamicNode.ParentNode?.Equals(parentNode) == true &&
                            !siteMap.GetChildNodes(parentNode).Contains(dynamicNode))
                        {
                            siteMap.AddNode(dynamicNode, parentNode);
                        }
                    }
                }
                else
                {
                    siteMap.AddNode(nodeToAdd, parentForNode);
                }
            }
            else
            {
                nodesToProcessLater.Add(nodeToAdd, assemblyNode.SiteMapNodeAttribute?.ParentKey);
            }
        }

        // Process list of nodes that did not have a parent defined.
        // If this does not succeed at this time, parent will default to root node.
        if (parentNode != null)
        {
            foreach (var nodeToAdd in nodesToProcessLater)
            {
                var parentForNode = siteMap.FindSiteMapNodeFromKey(nodeToAdd.Value);
                if (parentForNode == null)
                {
                    var temp = nodesToProcessLater.Keys.FirstOrDefault(t => t.Key == nodeToAdd.Value);
                    if (temp != null)
                    {
                        parentNode = temp;
                    }
                }

                if (parentForNode == null)
                {
                    continue;
                }

                if (nodeToAdd.Key.HasDynamicNodeProvider)
                {
                    var dynamicNodesForChildNode =
                        _dynamicNodeBuilder.BuildDynamicNodesFor(siteMap, nodeToAdd.Key, parentForNode);
                    foreach (var dynamicNode in dynamicNodesForChildNode)
                    {
                        // Verify parent/child relation
                        if (dynamicNode.ParentNode?.Equals(parentNode) == true &&
                            !siteMap.GetChildNodes(parentNode).Contains(dynamicNode))
                        {
                            siteMap.AddNode(dynamicNode, parentNode);
                        }
                    }
                }
                else
                {
                    siteMap.AddNode(nodeToAdd.Key, parentForNode);
                }
            }
        }

        return parentNode;
    }

    /// <summary>
    ///     Gets the site map node from MVC site map node attribute.
    /// </summary>
    /// <param name="siteMap">The site map.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="type">The type.</param>
    /// <param name="methodInfo">The method info.</param>
    /// <returns></returns>
    protected virtual ISiteMapNode? GetSiteMapNodeFromMvcSiteMapNodeAttribute(ISiteMap siteMap,
        IMvcSiteMapNodeAttribute? attribute, Type? type, MethodInfo? methodInfo)
    {
        if (!string.IsNullOrEmpty(attribute?.SiteMapCacheKey))
        {
            // Return null if the attribute doesn't apply to this cache key
            var mapCacheKey = SiteMapCacheKey;
            if (mapCacheKey?.Equals(attribute?.SiteMapCacheKey) == false)
            {
                return null;
            }
        }

        if (methodInfo == null) // try to find Index action
        {
            var ms = type?.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public,
                (mi, _) => mi != null && string.Equals(mi.Name, "Index"), null);
            if (ms != null)
            {
                foreach (var m in ms.OfType<MethodInfo>())
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
        if (!string.IsNullOrEmpty(attribute?.AreaName))
        {
            area = attribute?.AreaName;
        }

        if (string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(attribute?.Area))
        {
            area = attribute?.Area;
        }

        // Determine area (will only work if controller is defined as [<Anything>.]Areas.<Area>.Controllers.<AnyController>)
        if (string.IsNullOrEmpty(area))
        {
            var m = Regex.Match(type?.Namespace ?? string.Empty,
                @"(?:[^\.]+\.|\s+|^)Areas\.(?<areaName>[^\.]+)\.Controllers");
            if (m.Success)
            {
                area = m.Groups["areaName"].Value;
            }
        }

        // Determine controller and (index) action
        var controller = type?.Name.Substring(0, type.Name.IndexOf("Controller", StringComparison.Ordinal));
        var action = (methodInfo?.Name) ?? "Index";
        if (methodInfo != null)
        {
            // handle ActionNameAttribute
            if (methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true).FirstOrDefault() is
                ActionNameAttribute actionNameAttribute)
            {
                action = actionNameAttribute.Name;
            }
        }

        var httpMethod = string.IsNullOrEmpty(attribute?.HttpMethod)
            ? nameof(HttpVerbs.Get).ToUpperInvariant()
            : attribute?.HttpMethod.ToUpperInvariant();

        // Handle title
        var title = attribute?.Title;

        // Handle implicit resources
        var implicitResourceKey = attribute?.ResourceKey;

        // Generate key for node
        var key = _nodeKeyGenerator.GenerateKey(
            null,
            attribute?.Key,
            "",
            title,
            area,
            controller, action, httpMethod,
            attribute?.Clickable ?? true);

        var siteMapNode = _siteMapNodeFactory.Create(siteMap, key, implicitResourceKey);

        // Assign defaults
        siteMapNode.Title = title;
        siteMapNode.Description = attribute.Description;
        siteMapNode.Attributes.AddRange(attribute.Attributes, false);
        siteMapNode.Roles.AddRange(attribute.Roles);
        siteMapNode.Clickable = attribute.Clickable;
        siteMapNode.VisibilityProvider = attribute.VisibilityProvider;
        siteMapNode.DynamicNodeProvider = attribute.DynamicNodeProvider;
        siteMapNode.ImageUrl = attribute.ImageUrl;
        siteMapNode.ImageUrlProtocol = attribute.ImageUrlProtocol;
        siteMapNode.ImageUrlHostName = attribute.ImageUrlHostName;
        siteMapNode.TargetFrame = attribute.TargetFrame;
        siteMapNode.HttpMethod = httpMethod;
        if (!string.IsNullOrEmpty(attribute.Url))
        {
            siteMapNode.Url = attribute.Url;
        }

        siteMapNode.CacheResolvedUrl = attribute.CacheResolvedUrl;
        siteMapNode.IncludeAmbientValuesInUrl = attribute.IncludeAmbientValuesInUrl;
        siteMapNode.Protocol = attribute.Protocol;
        siteMapNode.HostName = attribute.HostName;
        siteMapNode.CanonicalKey = attribute.CanonicalKey;
        siteMapNode.CanonicalUrl = attribute.CanonicalUrl;
        siteMapNode.CanonicalUrlProtocol = attribute.CanonicalUrlProtocol;
        siteMapNode.CanonicalUrlHostName = attribute.CanonicalUrlHostName;
        siteMapNode.MetaRobotsValues.AddRange(attribute.MetaRobotsValues);
        siteMapNode.LastModifiedDate = string.IsNullOrEmpty(attribute.LastModifiedDate)
            ? DateTime.MinValue
            : DateTime.Parse(attribute.LastModifiedDate, CultureInfo.InvariantCulture);
        siteMapNode.ChangeFrequency = attribute.ChangeFrequency;
        siteMapNode.UpdatePriority = attribute.UpdatePriority;
        siteMapNode.Order = attribute.Order;

        // Handle route details
        siteMapNode.Route = attribute.Route;
        siteMapNode.RouteValues.AddRange(attribute.Attributes, false);
        siteMapNode.PreservedRouteParameters.AddRange(attribute.PreservedRouteParameters, [',', ';']);
        siteMapNode.UrlResolver = attribute.UrlResolver;

        // Specified area, controller and action properties will override any 
        // provided in the attributes collection.
        if (!string.IsNullOrEmpty(area))
        {
            siteMapNode.RouteValues.Add("area", area);
        }

        if (!string.IsNullOrEmpty(controller))
        {
            siteMapNode.RouteValues.Add("controller", controller);
        }

        if (!string.IsNullOrEmpty(action))
        {
            siteMapNode.RouteValues.Add("action", action);
        }

        return siteMapNode;
    }
}
