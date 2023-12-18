using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Web.Compilation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcSiteMapProvider.Web.Mvc
{
    /// <summary>
    /// ControllerTypeResolver class
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class ControllerTypeResolver
        : IControllerTypeResolver
    {
        protected readonly IEnumerable<string> areaNamespacesToIgnore;

        protected readonly IBuildManager buildManager;

        protected readonly IControllerBuilder controllerBuilder;

        protected readonly RouteCollection routes;

        private readonly object synclock = new object();

        public ControllerTypeResolver(
                                                    IEnumerable<string> areaNamespacesToIgnore,
            RouteCollection routes,
            IControllerBuilder controllerBuilder,
            IBuildManager buildManager
            )
        {
            this.areaNamespacesToIgnore = areaNamespacesToIgnore ?? throw new ArgumentNullException(nameof(areaNamespacesToIgnore));
            this.routes = routes ?? throw new ArgumentNullException(nameof(routes));
            this.controllerBuilder = controllerBuilder ?? throw new ArgumentNullException(nameof(controllerBuilder));
            this.buildManager = buildManager ?? throw new ArgumentNullException(nameof(buildManager));

            Cache = new ConcurrentDictionary<string, Type>();
        }

        /// <summary>
        /// Gets or sets the assembly cache.
        /// </summary>
        /// <value>The assembly cache.</value>
        protected Lazy<Dictionary<string, ILookup<string, Type>>> AssemblyCache { get; private set; }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>The cache.</value>
        protected ConcurrentDictionary<string, Type> Cache { get; }

        /// <summary>
        /// Resolves the type of the controller.
        /// </summary>
        /// <param name="areaName">Name of the area.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>Controller type</returns>
        public Type ResolveControllerType(string areaName, string controllerName)
        {
            string cacheKey = areaName + "_" + controllerName;

            // Try to get the value from the cache first
            if (Cache.TryGetValue(cacheKey, out Type cachedType))
            {
                return cachedType;
            }

            // Compute areaNamespaces only if necessary
            var areaNamespaces = FindNamespacesForArea(areaName, routes)?.Except(areaNamespacesToIgnore, StringComparer.OrdinalIgnoreCase).ToList();
            HashSet<string> namespaces = null;

            if (areaNamespaces?.Any() == true)
            {
                namespaces = new HashSet<string>(areaNamespaces, StringComparer.OrdinalIgnoreCase);
                if (string.IsNullOrEmpty(areaName))
                {
                    namespaces.UnionWith(controllerBuilder.DefaultNamespaces);
                }
            }
            else if (controllerBuilder.DefaultNamespaces.Any())
            {
                namespaces = new HashSet<string>(controllerBuilder.DefaultNamespaces, StringComparer.OrdinalIgnoreCase);
            }

            Type controllerType = GetControllerTypeWithinNamespaces(areaName, controllerName, namespaces);

            // Update the cache
            if (controllerType != null)
            {
                Cache.AddOrUpdate(cacheKey, controllerType, (_, __) => controllerType);
            }

            return controllerType;
        }

        /// <summary>
        /// Finds the namespaces for area.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="routes">The routes.</param>
        /// <returns>
        /// A namespaces for area represented as a <see cref="string"/> instance
        /// </returns>
        protected virtual IEnumerable<string> FindNamespacesForArea(string area, RouteCollection routes)
        {
            var namespacesForArea = new List<string>();
            var namespacesCommon = new List<string>();

            foreach (var route in routes.OfType<Route>().Where(r => r.DataTokens?["Namespaces"] != null))
            {
                // search for area-based namespaces
                if (route.DataTokens["area"]?.ToString().Equals(area, StringComparison.OrdinalIgnoreCase) == true)
                    namespacesForArea.AddRange((IEnumerable<string>)route.DataTokens["Namespaces"]);
                else if (route.DataTokens["area"] == null)
                    namespacesCommon.AddRange((IEnumerable<string>)route.DataTokens["Namespaces"]);
            }

            if (namespacesForArea.Count > 0)
            {
                return namespacesForArea;
            }
            else if (namespacesCommon.Count > 0)
            {
                return namespacesCommon;
            }

            return null;
        }

        /// <summary>
        /// Gets the controller type within namespaces.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns>
        /// A controller type within namespaces represented as a <see cref="Type"/> instance
        /// </returns>
        protected virtual Type GetControllerTypeWithinNamespaces(string area, string controller, HashSet<string> namespaces)
        {
            if (string.IsNullOrEmpty(controller) || controller?.Length == 0)
                return null;

            InitAssemblyCache();

            HashSet<Type> matchingTypes = new HashSet<Type>();
            if (AssemblyCache.Value.TryGetValue(controller, out ILookup<string, Type> nsLookup))
            {
                // this friendly name was located in the cache, now cycle through namespaces
                if (namespaces != null)
                {
                    foreach (string requestedNamespace in namespaces)
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

            if (matchingTypes.Count == 1)
            {
                return matchingTypes.First();
            }
            else if (matchingTypes.Count > 1)
            {
                string typeNames = Environment.NewLine + Environment.NewLine;
                foreach (var matchingType in matchingTypes)
                    typeNames += matchingType.FullName + Environment.NewLine;
                typeNames += Environment.NewLine;

                throw new AmbiguousControllerException(string.Format(Resources.Messages.AmbiguousControllerFoundMultipleControllers, controller, typeNames));
            }
            return null;
        }

        /// <summary>
        /// Gets the list of controller types.
        /// </summary>
        /// <returns></returns>
        protected virtual List<Type> GetListOfControllerTypes()
        {
            var controllerTypes = new HashSet<Type>();
            var assemblies = this.buildManager.GetReferencedAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] typesInAsm;
                try
                {
                    typesInAsm = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    typesInAsm = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in typesInAsm)
                {
                    if (type?.IsClass == true && type.IsPublic && !type.IsAbstract)
                    {
                        var typeName = type.Name;
                        if (typeName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) && typeof(IController).IsAssignableFrom(type))
                        {
                            controllerTypes.Add(type);
                        }
                    }
                }
            }
            return controllerTypes.ToList();
        }

        /// <summary>
        /// Determines whether namespace matches the specified requested namespace.
        /// </summary>
        /// <param name="requestedNamespace">The requested namespace.</param>
        /// <param name="targetNamespace">The target namespace.</param>
        /// <returns>
        /// 	<c>true</c> if is namespace matches the specified requested namespace; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNamespaceMatch(string requestedNamespace, string targetNamespace)
        {
            // degenerate cases
            if (requestedNamespace == null)
            {
                return false;
            }
            else if (requestedNamespace.Length == 0)
            {
                return true;
            }

            if (!requestedNamespace.EndsWith(".*", StringComparison.OrdinalIgnoreCase))
            {
                // looking for exact namespace match
                return string.Equals(requestedNamespace, targetNamespace, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
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
                else if (targetNamespace[requestedNamespace.Length] == '.')
                {
                    // good prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar.Baz"
                    return true;
                }
                else
                {
                    // bad prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar2"
                    return false;
                }
            }
        }

        /// <summary>
        /// Inits the assembly cache.
        /// </summary>
        private void InitAssemblyCache()
        {
            if (AssemblyCache == null)
            {
                lock (synclock)
                {
                    if (AssemblyCache == null)
                    {
                        AssemblyCache = new Lazy<Dictionary<string, ILookup<string, Type>>>(() =>
                        {
                            var controllerTypes = GetListOfControllerTypes();
                            var groupedByName = controllerTypes.GroupBy(
                                t => t.Name.Substring(0, t.Name.IndexOf("Controller")),
                                StringComparer.OrdinalIgnoreCase);

                            return groupedByName.ToDictionary(
                                g => g.Key,
                                g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase),
                                StringComparer.OrdinalIgnoreCase);
                        });
                    }
                }
            }
        }
    }
}