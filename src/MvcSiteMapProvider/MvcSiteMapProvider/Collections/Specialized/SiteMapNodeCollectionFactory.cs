namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// An abstract factory that can be used to create new instances of <see cref="T:MvcSiteMapProvider.ISiteMapNodeCollection"/>
    /// at runtime.
    /// </summary>
    public class SiteMapNodeCollectionFactory
        : ISiteMapNodeCollectionFactory
    {
        public virtual ISiteMapNodeCollection Create()
        {
            return new SiteMapNodeCollection();
        }

        public virtual ISiteMapNodeCollection CreateEmptyReadOnly()
        {
            return new ReadOnlySiteMapNodeCollection(new SiteMapNodeCollection());
        }

        public virtual ISiteMapNodeCollection CreateLockable(ISiteMap siteMap)
        {
            return new LockableSiteMapNodeCollection(siteMap);
        }

        public virtual ISiteMapNodeCollection CreateReadOnly(ISiteMapNodeCollection siteMapNodeCollection)
        {
            return new ReadOnlySiteMapNodeCollection(siteMapNodeCollection);
        }
    }
}