using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Web.Script.Serialization;
using System;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
/// An abstract factory that can be used to create new instances of 
/// <see cref="T:MvcSiteMapProvider.Collections.Specialized.RouteValueDictionary"/>
/// at runtime.
/// </summary>
public class RouteValueDictionaryFactory
    : IRouteValueDictionaryFactory
{
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

    protected readonly IRequestCache requestCache;
    protected readonly IReservedAttributeNameProvider reservedAttributeNameProvider;
    protected readonly IJsonToDictionaryDeserializer jsonToDictionaryDeserializer;

    #region IRouteValueDictionaryFactory Members

    public IRouteValueDictionary Create(string siteMapNodeKey, string memberName, ISiteMap siteMap)
    {
        return new RouteValueDictionary(siteMapNodeKey, memberName, siteMap, this.reservedAttributeNameProvider, this.jsonToDictionaryDeserializer, this.requestCache);
    }

    #endregion
}