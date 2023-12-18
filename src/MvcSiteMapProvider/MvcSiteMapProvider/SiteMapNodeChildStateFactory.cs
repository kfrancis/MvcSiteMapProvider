using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Globalization;
using System;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Abstract factory for creating new instances of types required by the <see cref="T:MvcSiteMapProvider.SiteMapNode"/>
    /// at runtime.
    /// </summary>
    public class SiteMapNodeChildStateFactory
        : ISiteMapNodeChildStateFactory
    {
        protected readonly IAttributeDictionaryFactory attributeDictionaryFactory;

        protected readonly IRouteValueDictionaryFactory routeValueDictionaryFactory;

        public SiteMapNodeChildStateFactory(
                            IAttributeDictionaryFactory attributeDictionaryFactory,
            IRouteValueDictionaryFactory routeValueDictionaryFactory
            )
        {
            this.attributeDictionaryFactory = attributeDictionaryFactory ?? throw new ArgumentNullException(nameof(attributeDictionaryFactory));
            this.routeValueDictionaryFactory = routeValueDictionaryFactory ?? throw new ArgumentNullException(nameof(routeValueDictionaryFactory));
        }

        public virtual IAttributeDictionary CreateAttributeDictionary(string siteMapNodeKey, string memberName, ISiteMap siteMap, ILocalizationService localizationService)
        {
            return attributeDictionaryFactory.Create(siteMapNodeKey, memberName, siteMap, localizationService);
        }

        public virtual IMetaRobotsValueCollection CreateMetaRobotsValueCollection(ISiteMap siteMap)
        {
            return new MetaRobotsValueCollection(siteMap);
        }

        public virtual IPreservedRouteParameterCollection CreatePreservedRouteParameterCollection(ISiteMap siteMap)
        {
            return new PreservedRouteParameterCollection(siteMap);
        }

        public virtual IRoleCollection CreateRoleCollection(ISiteMap siteMap)
        {
            return new RoleCollection(siteMap);
        }

        public virtual IRouteValueDictionary CreateRouteValueDictionary(string siteMapNodeKey, string memberName, ISiteMap siteMap)
        {
            return routeValueDictionaryFactory.Create(siteMapNodeKey, memberName, siteMap);
        }
    }
}