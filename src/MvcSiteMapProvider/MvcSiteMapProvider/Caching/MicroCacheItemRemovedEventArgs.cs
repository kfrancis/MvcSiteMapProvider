using System;

namespace MvcSiteMapProvider.Caching;

/// <summary>
///     A specialized <see cref="T:System.EventArgs" /> superclass that provides
///     access to the object instance that was removed from the cache.
/// </summary>
public class MicroCacheItemRemovedEventArgs<T>
    : EventArgs
{
    public MicroCacheItemRemovedEventArgs(string key, T item)
    {
        Key = key;
        Item = item;
    }

    public string Key { get; }
    public T Item { get; private set; }
}
