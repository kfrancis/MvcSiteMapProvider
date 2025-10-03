using System;
using System.Collections.Generic;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Collections;

/// <summary>
///     Generic list that is aware of the ISiteMap interface and can be made read-only
///     depending on the IsReadOnly property of ISiteMap.
/// </summary>
public class LockableList<T>
    : List<T>
{
    private readonly ISiteMap _siteMap;

    protected LockableList(ISiteMap siteMap)
    {
        _siteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
    }

    public virtual bool IsReadOnly => _siteMap.IsReadOnly;


    // Property access to internal list
    private LockableList<T> List => this;

    /// <summary>
    ///     Adds an object to the end of the <see cref="T:LockableList" />
    /// </summary>
    /// <param name="item">The item to add to the list.</param>
    public new virtual void Add(T item)
    {
        ThrowIfReadOnly();
        base.Add(item);
    }

    public new virtual void AddRange(IEnumerable<T> collection)
    {
        ThrowIfReadOnly();
        base.AddRange(collection);
    }

    public new virtual void Clear()
    {
        ThrowIfReadOnly();
        base.Clear();
    }

    public new virtual void Insert(int index, T item)
    {
        ThrowIfReadOnly();
        base.Insert(index, item);
    }

    public new virtual void InsertRange(int index, IEnumerable<T> collection)
    {
        ThrowIfReadOnly();
        base.InsertRange(index, collection);
    }

    public new virtual bool Remove(T item)
    {
        ThrowIfReadOnly();
        return base.Remove(item);
    }

    public new virtual int RemoveAll(Predicate<T> match)
    {
        ThrowIfReadOnly();
        return base.RemoveAll(match);
    }

    public new virtual void RemoveAt(int index)
    {
        ThrowIfReadOnly();
        base.RemoveAt(index);
    }

    public new virtual void RemoveRange(int index, int count)
    {
        ThrowIfReadOnly();
        base.RemoveRange(index, count);
    }

    public new virtual void Reverse()
    {
        ThrowIfReadOnly();
        base.Reverse();
    }

    public new virtual void Reverse(int index, int count)
    {
        ThrowIfReadOnly();
        base.Reverse(index, count);
    }

    public new virtual void Sort()
    {
        ThrowIfReadOnly();
        base.Sort();
    }

    public new virtual void Sort(Comparison<T> comparison)
    {
        ThrowIfReadOnly();
        base.Sort(comparison);
    }

    public new virtual void Sort(IComparer<T> comparer)
    {
        ThrowIfReadOnly();
        base.Sort(comparer);
    }

    public new virtual void Sort(int index, int count, IComparer<T> comparer)
    {
        ThrowIfReadOnly();
        base.Sort(index, count, comparer);
    }

    public new virtual void TrimExcess()
    {
        ThrowIfReadOnly();
        base.TrimExcess();
    }

    public virtual void CopyTo(IList<T> destination)
    {
        foreach (var item in List)
        {
            if (item != null && !item.GetType().IsPointer)
            {
                destination.Add(item);
            }
            else
            {
                throw new NotSupportedException(Messages.CopyOperationDoesNotSupportReferenceTypes);
            }
        }
    }

    protected virtual void ThrowIfReadOnly()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(string.Format(Messages.SiteMapReadOnly));
        }
    }
}
