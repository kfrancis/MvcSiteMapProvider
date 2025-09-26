using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace MvcSiteMapProvider.Web.Routing
{
    public static class RouteDataExtensions
    {
        /// <summary>
        ///     Retrieves the value with the specified identifier. Unlike GetRequiredString, this
        ///     method does not throw an exception if the value does not exist or is null.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <param name="valueName">The key of the value to retrieve.</param>
        /// <returns>The value with the specified key, or an empty string if not found.</returns>
        public static string? GetOptionalString(this RouteData routeData, string valueName)
        {
            if (routeData.Values.TryGetValue(valueName, out var value))
            {
                return value as string;
            }

            if (!routeData.Values.TryGetValue("MS_DirectRouteMatches", out var dataValue))
            {
                return string.Empty;
            }

            if (((IEnumerable<RouteData>)dataValue).First().Values.TryGetValue(valueName, out value))
            {
                return value as string;
            }

            return string.Empty;
        }

        /// <summary>
        ///     Retrieves the area name.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <returns>The area name, or an empty string if not found.</returns>
        public static string? GetAreaName(this RouteData routeData)
        {
            if (routeData.DataTokens.TryGetValue("area", out var value))
            {
                return value as string;
            }

            if (!routeData.Values.TryGetValue("MS_DirectRouteMatches", out var dataValue))
            {
                return string.Empty;
            }

            if (((IEnumerable<RouteData>)dataValue).First().DataTokens.TryGetValue("area", out value))
            {
                return value as string;
            }

            return string.Empty;
        }

        /// <summary>
        ///     Merges a set of key value pairs with the current DataTokens dictionary. Overwrites any duplicate keys.
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <param name="dataTokens">A dictionary of key value pairs to merge.</param>
        public static void MergeDataTokens(this RouteData? routeData, IDictionary<string, object> dataTokens)
        {
            if (routeData == null)
            {
                return;
            }

            foreach (var pair in dataTokens)
            {
                routeData.DataTokens[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        ///     Adds the MvcCodeRouting.RouteContext DataToken necessary for interoperability
        ///     with the MvcCodeRouting library https://github.com/maxtoroq/MvcCodeRouting
        /// </summary>
        /// <param name="routeData">The route data.</param>
        /// <param name="node">The current site map node.</param>
        internal static void SetMvcCodeRoutingContext(this RouteData? routeData, ISiteMapNode node)
        {
            if (routeData == null)
            {
                return;
            }

            var controllerType = node.SiteMap.ResolveControllerType(node.Area, node.Controller);

            // Fix for #416 - If Areas are misconfigured, controllerType may be null. Since MvcCodeRouting
            // doesn't work in conjunction with Areas anyway, we are going to ignore this here.
            if (controllerType == null)
            {
                return;
            }

            var mvcCodeRoutingRouteContext = GetMvcCodeRoutingRouteContext(controllerType, node.Controller);
            routeData.DataTokens["MvcCodeRouting.RouteContext"] = mvcCodeRoutingRouteContext;
        }

        private static string GetMvcCodeRoutingRouteContext(Type controllerType, string controllerName)
        {
            var controllerNamespace = controllerType.Namespace;

            if (controllerNamespace == null)
            {
                return string.Empty;
            }

            var controllersIndex = controllerNamespace.LastIndexOf(".Controllers.", StringComparison.Ordinal);
            if (controllersIndex == -1)
            {
                // this is a top level controller
                return string.Empty;
            }

            // for example if:
            // controllerNamespace = "DemoApp.Controllers.Sub1.Sub2.Sub3"

            // then selfNamespace is "Sub1.Sub2.Sub3"
            var selfNamespace = controllerNamespace.Substring(controllersIndex + 13);

            // selfNamespace = parentNamespace + "." + selfNamespaceLast
            var parentIndex = selfNamespace.LastIndexOf('.');
            var parentNamespace = string.Empty;
            var selfNamespaceLast = selfNamespace;

            if (parentIndex == -1)
            {
                return controllerName.Equals(selfNamespaceLast, StringComparison.Ordinal)
                    ? parentNamespace // default
                    : selfNamespace; // non-default
            }

            // "Sub1.Sub2"
            parentNamespace = selfNamespace.Substring(0, parentIndex);
            // "Sub3"
            selfNamespaceLast = selfNamespace.Substring(parentIndex + 1);

            // check for default controller
            return controllerName.Equals(selfNamespaceLast, StringComparison.Ordinal)
                ? parentNamespace // default
                : selfNamespace; // non-default
        }
    }
}
