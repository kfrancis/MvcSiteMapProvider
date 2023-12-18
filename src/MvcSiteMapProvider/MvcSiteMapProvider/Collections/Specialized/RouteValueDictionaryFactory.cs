using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Web.Script.Serialization;
using System;

namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of
    /// <see cref="T:MvcSiteMapProvider.Collections.Specialized.RouteValueDictionary"/>
    /// at runtime.
    /// </summary>
    public class RouteValueDictionaryFactory
        : IRouteValueDictionaryFactory
    {
        protected readonly IJsonToDictionaryDeserializer jsonToDictionaryDeserializer;

        protected readonly IRequestCache requestCache;

        protected readonly IReservedAttributeNameProvider reservedAttributeNameProvider;

        public RouteValueDictionaryFactory(
                                    IRequestCache requestCache,
            IReservedAttributeNameProvider reservedAttributeNameProvider,
            IJsonToDictionaryDeserializer jsonToDictionaryDeserializer
            )
        {
            this.requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
            this.reservedAttributeNameProvider = reservedAttributeNameProvider ?? throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
            this.jsonToDictionaryDeserializer = jsonToDictionaryDeserializer ?? throw new ArgumentNullException(nameof(jsonToDictionaryDeserializer));
        }

        public IRouteValueDictionary Create(string siteMapNodeKey, string memberName, ISiteMap siteMap)
        {
            return new RouteValueDictionary(siteMapNodeKey, memberName, siteMap, reservedAttributeNameProvider, jsonToDictionaryDeserializer, requestCache);
        }
    }
}