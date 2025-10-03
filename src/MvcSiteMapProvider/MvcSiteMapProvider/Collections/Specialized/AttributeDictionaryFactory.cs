using System;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web.Script.Serialization;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     An abstract factory that can be used to create new instances of
///     <see cref="T:MvcSiteMapProvider.Collections.Specialized.AttributeDictionary" />
///     at runtime.
/// </summary>
public class AttributeDictionaryFactory
    : IAttributeDictionaryFactory
{
    private readonly IJsonToDictionaryDeserializer _jsonToDictionaryDeserializer;

    private readonly IRequestCache _requestCache;
    private readonly IReservedAttributeNameProvider _reservedAttributeNameProvider;

    public AttributeDictionaryFactory(
        IRequestCache requestCache,
        IReservedAttributeNameProvider reservedAttributeNameProvider,
        IJsonToDictionaryDeserializer jsonToDictionaryDeserializer
    )
    {
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        _reservedAttributeNameProvider = reservedAttributeNameProvider ??
                                         throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _jsonToDictionaryDeserializer = jsonToDictionaryDeserializer ??
                                        throw new ArgumentNullException(nameof(jsonToDictionaryDeserializer));
    }

    public virtual IAttributeDictionary Create(string siteMapNodeKey, string memberName, ISiteMap siteMap,
        ILocalizationService localizationService)
    {
        return new AttributeDictionary(siteMapNodeKey, memberName, siteMap, localizationService,
            _reservedAttributeNameProvider, _jsonToDictionaryDeserializer, _requestCache);
    }
}
