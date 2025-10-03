using System;
using System.Collections;
using System.Collections.Generic;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     A specialized collection that provides a read-only wrapper for a
///     <see cref="T:MvcSiteMapProvider.ISiteMapNodeCollection" />.
/// </summary>
public class ReadOnlySiteMapNodeCollection
    : ISiteMapNodeCollection
{
    private readonly ISiteMapNodeCollection _siteMapNodeCollection;

    public ReadOnlySiteMapNodeCollection(
        ISiteMapNodeCollection siteMapNodeCollection
    )
    {
        _siteMapNodeCollection =
            siteMapNodeCollection ?? throw new ArgumentNullException(nameof(siteMapNodeCollection));
    }

    public void AddRange(IEnumerable<ISiteMapNode> collection)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public void RemoveRange(int index, int count)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public int IndexOf(ISiteMapNode item)
    {
        return _siteMapNodeCollection.IndexOf(item);
    }

    public void Insert(int index, ISiteMapNode item)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public void RemoveAt(int index)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public ISiteMapNode this[int index]
    {
        get => _siteMapNodeCollection[index];
        set => throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public void Add(ISiteMapNode item)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public void Clear()
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public bool Contains(ISiteMapNode item)
    {
        return _siteMapNodeCollection.Contains(item);
    }

    public void CopyTo(ISiteMapNode[] array, int arrayIndex)
    {
        _siteMapNodeCollection.CopyTo(array, arrayIndex);
    }

    public int Count => _siteMapNodeCollection.Count;

    public bool IsReadOnly => true;

    public bool Remove(ISiteMapNode item)
    {
        throw new NotSupportedException(Messages.CollectionReadOnly);
    }

    public IEnumerator<ISiteMapNode> GetEnumerator()
    {
        return _siteMapNodeCollection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _siteMapNodeCollection.GetEnumerator();
    }
}
