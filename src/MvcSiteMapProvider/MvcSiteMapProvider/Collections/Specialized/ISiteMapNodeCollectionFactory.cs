namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     Contract for an abstract factory that creates new instances of
///     <see cref="T:ISiteMapNodeCollection" /> at runtime.
/// </summary>
public interface ISiteMapNodeCollectionFactory
{
    ISiteMapNodeCollection Create();
    ISiteMapNodeCollection CreateLockable(ISiteMap siteMap);
    ISiteMapNodeCollection CreateReadOnly(ISiteMapNodeCollection siteMapNodeCollection);
    ISiteMapNodeCollection CreateEmptyReadOnly();
}
