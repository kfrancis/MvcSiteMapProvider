using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Web.Compilation;

namespace MvcSiteMapProvider.Web.Mvc;

/// <summary>
///     ControllerTypeResolver class
/// </summary>
[ExcludeFromAutoRegistration]
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public class ControllerTypeResolver
    : IControllerTypeResolver
{
    private readonly IEnumerable<string> _areaNamespacesToIgnore;
    private readonly IControllerBuilder _controllerBuilder;
    private readonly RouteCollection _routes;


    private readonly object _synclock = new();
    protected readonly IBuildManager BuildManager;

    public ControllerTypeResolver(
        IEnumerable<string> areaNamespacesToIgnore,
        RouteCollection routes,
        IControllerBuilder controllerBuilder,
        IBuildManager buildManager
    )
    {
        _areaNamespacesToIgnore = areaNamespacesToIgnore ??
                                  throw new ArgumentNullException(nameof(areaNamespacesToIgnore));
        _routes = routes ?? throw new ArgumentNullException(nameof(routes));
        _controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
        BuildManager = buildManager ?? throw new ArgumentNullException(nameof(buildManager));

        Cache = new ThreadSafeDictionary<string, Type>();
    }

    /// <summary>
    ///     Gets or sets the cache.
    /// </summary>
    /// <value>The cache.</value>
    private ThreadSafeDictionary<string, Type> Cache { get; }

    /// <summary>
    ///     Gets or sets the assembly cache.
    /// </summary>
    /// <value>The assembly cache.</value>
    private Dictionary<string, ILookup<string, Type>>? AssemblyCache { get; set; }

    /// <summary>
    ///     Resolves the type of the controller.
    /// </summary>
    /// <param name="areaName">Name of the area.</param>
    /// <param name="controllerName">Name of the controller.</param>
    /// <returns>Controller type</returns>
    public Type ResolveControllerType(string areaName, string controllerName)
    {
        // Is the type cached?
        var cacheKey = areaName + "_" + controllerName;
        if (Cache.TryGetValue(cacheKey, out var type))
        {
            return type;
        }

        // Find controller details
        var areaNamespaces = FindNamespacesForArea(areaName, _routes);

        var area = areaName;
        var controller = controllerName;

        // Find controller type
        HashSet<string>? namespaces = null;
        if (areaNamespaces != null)
        {
            areaNamespaces = (from ns in areaNamespaces
                              where ns != "Elmah.Mvc"
                              where !_areaNamespacesToIgnore.Contains(ns)
                              select ns).ToList();
            if (areaNamespaces.Any())
            {
                namespaces = new HashSet<string>(areaNamespaces, StringComparer.OrdinalIgnoreCase);
                if (string.IsNullOrEmpty(areaName))
                {
                    namespaces =
                        new HashSet<string>(
                            namespaces.Union(_controllerBuilder.DefaultNamespaces,
                                StringComparer.OrdinalIgnoreCase),
                            StringComparer.OrdinalIgnoreCase);
                }
            }
        }
        else if (_controllerBuilder.DefaultNamespaces.Count > 0)
        {
            namespaces = _controllerBuilder.DefaultNamespaces;
        }

        var controllerType = GetControllerTypeWithinNamespaces(area, controller, namespaces);

        // Cache the result
        Cache.Add(cacheKey, controllerType);

        // Return
        return controllerType;
    }

    /// <summary>
    ///     Finds the namespaces for area.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <param name="routeCollection">The routes.</param>
    /// <returns>
    ///     A namespaces for area represented as a <see cref="string" /> instance
    /// </returns>
    protected virtual IEnumerable<string>? FindNamespacesForArea(string area, RouteCollection routeCollection)
    {
        var namespacesForArea = new List<string>();
        var namespacesCommon = new List<string>();

        foreach (var route in routeCollection.OfType<Route>()
                     .Where(r => r.DataTokens != null && r.DataTokens["Namespaces"] != null))
        {
            // search for area-based namespaces
            if (route.DataTokens["area"] != null &&
                route.DataTokens["area"].ToString().Equals(area, StringComparison.OrdinalIgnoreCase))
            {
                namespacesForArea.AddRange((IEnumerable<string>)route.DataTokens["Namespaces"]);
            }
            else if (route.DataTokens["area"] == null)
            {
                namespacesCommon.AddRange((IEnumerable<string>)route.DataTokens["Namespaces"]);
            }
        }

        if (namespacesForArea.Count > 0)
        {
            return namespacesForArea;
        }

        return namespacesCommon.Count > 0 ? namespacesCommon : null;
    }

    /// <summary>
    ///     Inits the assembly cache.
    /// </summary>
    private void InitAssemblyCache()
    {
        if (AssemblyCache != null)
        {
            return;
        }

        lock (_synclock)
        {
            if (AssemblyCache != null)
            {
                return;
            }

            var controllerTypes = GetListOfControllerTypes();
            var groupedByName = controllerTypes.GroupBy(
                t => t.Name.Substring(0, t.Name.IndexOf("Controller", StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);
            AssemblyCache = groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    ///     Gets the list of controller types.
    /// </summary>
    /// <returns></returns>
    protected virtual List<Type> GetListOfControllerTypes()
    {
        var result = new List<Type>(512);
        var assemblies = BuildManager.GetReferencedAssemblies();
        var controllerType = typeof(IController);

        foreach (Assembly assembly in assemblies)
        {
            // Skip assemblies that definitely won't contain controllers
            if (IsSystemAssembly(assembly))
            {
                continue;
            }

            Type?[] typesInAsm;
            try
            {
                typesInAsm = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                typesInAsm = ex.Types;
            }

            if (typesInAsm == null)
            {
                continue;
            }

            // Process types in a single pass to reduce overhead
            foreach (var t in typesInAsm)
            {
                // Early null check
                if (t == null)
                {
                    continue;
                }

                // Combined checks to short-circuit faster
                // Check cheapest conditions first
                if (!t.IsClass || t.IsAbstract || !t.IsPublic)
                {
                    continue;
                }

                // Check if it's a controller type (more expensive check)
                if (!controllerType.IsAssignableFrom(t))
                {
                    continue;
                }

                // Check name ends with "Controller" using EndsWith instead of IndexOf
                var name = t.Name;
                if (name.Length > 10 && name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(t);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Determines if an assembly is a system assembly that won't contain controllers.
    /// </summary>
    /// <param name="assembly">The assembly to check.</param>
    /// <returns>True if the assembly is a system assembly; otherwise, false.</returns>
    private static bool IsSystemAssembly(Assembly assembly)
    {
        // Cache the assembly name to avoid multiple allocations
        var assemblyName = assembly.FullName;

        if (string.IsNullOrEmpty(assemblyName))
        {
            return false;
        }

        // Check for common system assemblies that won't have controllers
        // Using StartsWith with ordinal comparison for performance
        return assemblyName.StartsWith("mscorlib,", StringComparison.Ordinal)
            || assemblyName.StartsWith("System,", StringComparison.Ordinal)
            || assemblyName.StartsWith("System.", StringComparison.Ordinal)
            || assemblyName.StartsWith("Microsoft.CSharp,", StringComparison.Ordinal)
            || assemblyName.StartsWith("netstandard,", StringComparison.Ordinal)
            || assemblyName.StartsWith("Newtonsoft.Json,", StringComparison.Ordinal)
            || assemblyName.StartsWith("EntityFramework,", StringComparison.Ordinal)
            || assemblyName.StartsWith("EntityFramework.", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Determines whether namespace matches the specified requested namespace.
    /// </summary>
    /// <param name="requestedNamespace">The requested namespace.</param>
    /// <param name="targetNamespace">The target namespace.</param>
    /// <returns>
    ///     <c>true</c> if is namespace matches the specified requested namespace; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsNamespaceMatch(string? requestedNamespace, string targetNamespace)
    {
        // degenerate cases
        if (requestedNamespace == null)
        {
            return false;
        }

        if (requestedNamespace.Length == 0)
        {
            return true;
        }

        if (!requestedNamespace.EndsWith(".*", StringComparison.OrdinalIgnoreCase))
        {
            // looking for exact namespace match
            return string.Equals(requestedNamespace, targetNamespace, StringComparison.OrdinalIgnoreCase);
        }

        // looking for exact or sub-namespace match
        requestedNamespace = requestedNamespace.Substring(0, requestedNamespace.Length - ".*".Length);
        if (!targetNamespace.StartsWith(requestedNamespace, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (requestedNamespace.Length == targetNamespace.Length)
        {
            // exact match
            return true;
        }

        if (targetNamespace[requestedNamespace.Length] == '.')
        {
            // good prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar.Baz"
            return true;
        }

        // bad prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar2"
        return false;
    }

    /// <summary>
    ///     Gets the controller type within namespaces.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="namespaces">The namespaces.</param>
    /// <returns>
    ///     A controller type within namespaces represented as a <see cref="Type" /> instance
    /// </returns>
    protected virtual Type? GetControllerTypeWithinNamespaces(string area, string controller,
        HashSet<string>? namespaces)
    {
        if (string.IsNullOrEmpty(controller) || controller == "")
        {
            return null;
        }

        InitAssemblyCache();

        var matchingTypes = new HashSet<Type>();
        if (AssemblyCache != null && AssemblyCache.TryGetValue(controller, out var nsLookup))
        {
            // this friendly name was located in the cache, now cycle through namespaces
            if (namespaces != null)
            {
                foreach (var requestedNamespace in namespaces)
                {
                    foreach (var targetNamespaceGrouping in nsLookup)
                    {
                        if (IsNamespaceMatch(requestedNamespace, targetNamespaceGrouping.Key))
                        {
                            matchingTypes.UnionWith(targetNamespaceGrouping);
                        }
                    }
                }
            }
            else
            {
                // if the namespaces parameter is null, search *every* namespace
                foreach (var nsGroup in nsLookup)
                {
                    matchingTypes.UnionWith(nsGroup);
                }
            }
        }

        switch (matchingTypes.Count)
        {
            case 1:
                return matchingTypes.First();

            case > 1:
                {
                    var typeNames = Environment.NewLine + Environment.NewLine;
                    foreach (var matchingType in matchingTypes)
                    {
                        typeNames += matchingType.FullName + Environment.NewLine;
                    }

                    typeNames += Environment.NewLine;

                    throw new AmbiguousControllerException(
                        string.Format(Messages.AmbiguousControllerFoundMultipleControllers, controller, typeNames));
                }

            default:
                return null;
        }
    }
}
