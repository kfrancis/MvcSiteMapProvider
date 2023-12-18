using MvcSiteMapProvider.DI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider.Builder
{
    /// <summary>
    /// Provides information whether a given attribute name is a reserved name or it is
    /// safe to use the attribute in a SiteMapNode's Attributes or RouteValues dictionaries.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class ReservedAttributeNameProvider
        : IReservedAttributeNameProvider
    {
        protected readonly IEnumerable<string> AttributesToIgnore;

        public ReservedAttributeNameProvider(
                    IEnumerable<string> attributesToIgnore
            )
        {
            this.AttributesToIgnore = attributesToIgnore ?? throw new ArgumentNullException(nameof(attributesToIgnore));
        }

        public virtual bool IsRegularAttribute(string attributeName)
        {
            return !IsKnownAttribute(attributeName)
                && attributeName != "controller"
                && attributeName != "action"
                && attributeName != "area";
        }

        public virtual bool IsRouteAttribute(string attributeName)
        {
            return !IsKnownAttribute(attributeName)
                && attributeName != "visibility"
                && !AttributesToIgnore.Contains(attributeName)
                && !attributeName.StartsWith("data-");
        }

        protected virtual bool IsKnownAttribute(string attributeName)
        {
            return attributeName == "key"
                || attributeName == "order"
                || attributeName == "httpMethod"
                || attributeName == "resourceKey"
                || attributeName == "title"
                || attributeName == "description"
                || attributeName == "targetFrame"
                || attributeName == "imageUrl"
                || attributeName == "imageUrlProtocol"
                || attributeName == "imageUrlHostName"
                || attributeName == "roles"
                || attributeName == "lastModifiedDate"
                || attributeName == "changeFrequency"
                || attributeName == "updatePriority"
                || attributeName == "visibilityProvider"
                || attributeName == "dynamicNodeProvider"
                || attributeName == "clickable"
                || attributeName == "urlResolver"
                || attributeName == "url"
                || attributeName == "cacheResolvedUrl"
                || attributeName == "includeAmbientValuesInUrl"
                || attributeName == "protocol"
                || attributeName == "hostName"
                || attributeName == "canonicalKey"
                || attributeName == "canonicalUrl"
                || attributeName == "canonicalUrlProtocol"
                || attributeName == "canonicalUrlHostName"
                || attributeName == "metaRobotsValues"
                || attributeName == "route"
                || attributeName == "inheritedRouteParameters"
                || attributeName == "preservedRouteParameters";
        }
    }
}
