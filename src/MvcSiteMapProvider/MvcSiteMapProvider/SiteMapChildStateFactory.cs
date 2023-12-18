using MvcSiteMapProvider.Collections;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Matching;
using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider
{
    /// <summary>
    /// Abstract factory for creating new instances of types required by the <see cref="T:MvcSiteMapProvider.SiteMap"/>
    /// at runtime.
    /// </summary>
    public class SiteMapChildStateFactory
        : ISiteMapChildStateFactory
    {
        public SiteMapChildStateFactory(
            IGenericDictionaryFactory genericDictionaryFactory,
            ISiteMapNodeCollectionFactory siteMapNodeCollectionFactory,
            IUrlKeyFactory urlKeyFactory
            )
        {
            this.genericDictionaryFactory = genericDictionaryFactory ?? throw new ArgumentNullException(nameof(genericDictionaryFactory));
            this.siteMapNodeCollectionFactory = siteMapNodeCollectionFactory ?? throw new ArgumentNullException(nameof(siteMapNodeCollectionFactory));
            this.urlKeyFactory = urlKeyFactory ?? throw new ArgumentNullException(nameof(urlKeyFactory));
        }

        protected readonly IGenericDictionaryFactory genericDictionaryFactory;
        protected readonly ISiteMapNodeCollectionFactory siteMapNodeCollectionFactory;
        protected readonly IUrlKeyFactory urlKeyFactory;

        #region ISiteMapChildStateFactory Members

        public virtual IDictionary<ISiteMapNode, ISiteMapNodeCollection> CreateChildNodeCollectionDictionary()
        {
            return genericDictionaryFactory.Create<ISiteMapNode, ISiteMapNodeCollection>();
        }

        public virtual IDictionary<string, ISiteMapNode> CreateKeyDictionary()
        {
            return genericDictionaryFactory.Create<string, ISiteMapNode>();
        }

        public virtual IDictionary<ISiteMapNode, ISiteMapNode> CreateParentNodeDictionary()
        {
            return genericDictionaryFactory.Create<ISiteMapNode, ISiteMapNode>();
        }

        public virtual IDictionary<IUrlKey, ISiteMapNode> CreateUrlDictionary()
        {
            return genericDictionaryFactory.Create<IUrlKey, ISiteMapNode>();
        }

        public virtual IUrlKey CreateUrlKey(ISiteMapNode node)
        {
            return urlKeyFactory.Create(node);
        }

        public virtual IUrlKey CreateUrlKey(string relativeOrAbsoluteUrl, string hostName)
        {
            return urlKeyFactory.Create(relativeOrAbsoluteUrl, hostName);
        }

        public virtual ISiteMapNodeCollection CreateSiteMapNodeCollection()
        {
            return siteMapNodeCollectionFactory.Create();
        }

        public virtual ISiteMapNodeCollection CreateLockableSiteMapNodeCollection(ISiteMap siteMap)
        {
            return siteMapNodeCollectionFactory.CreateLockable(siteMap);
        }

        public virtual ISiteMapNodeCollection CreateReadOnlySiteMapNodeCollection(ISiteMapNodeCollection siteMapNodeCollection)
        {
            return siteMapNodeCollectionFactory.CreateReadOnly(siteMapNodeCollection);
        }

        public virtual ISiteMapNodeCollection CreateEmptyReadOnlySiteMapNodeCollection()
        {
            return siteMapNodeCollectionFactory.CreateEmptyReadOnly();
        }

        #endregion ISiteMapChildStateFactory Members
    }
}