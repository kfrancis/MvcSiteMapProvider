using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web.Script.Serialization;
using System;

namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.Collections.Specialized.AttributeDictionary"/>
    /// at runtime.
    /// </summary>
    public class AttributeDictionaryFactory
        : IAttributeDictionaryFactory
    {
        public AttributeDictionaryFactory(
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

        #region IAttributeDictionaryFactory Members

        public virtual IAttributeDictionary Create(string siteMapNodeKey, string memberName, ISiteMap siteMap, ILocalizationService localizationService)
        {
            return new AttributeDictionary(siteMapNodeKey, memberName, siteMap, localizationService, reservedAttributeNameProvider, jsonToDictionaryDeserializer, requestCache);
        }

        #endregion IAttributeDictionaryFactory Members
    }
}