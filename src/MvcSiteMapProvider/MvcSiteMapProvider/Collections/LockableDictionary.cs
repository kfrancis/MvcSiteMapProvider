using System;
using System.Collections.Generic;
using MvcSiteMapProvider.Resources;

namespace MvcSiteMapProvider.Collections;

/// <summary>
///     Generic dictionary that is aware of the ISiteMap interface and can be made read-only
///     depending on the IsReadOnly property of ISiteMap.
/// </summary>
public class LockableDictionary<TKey, TValue>
    : ObservableDictionary<TKey, TValue>
{
    protected readonly ISiteMap SiteMap;

    protected LockableDictionary(
        ISiteMap siteMap
    )
    {
        SiteMap = siteMap ?? throw new ArgumentNullException(nameof(siteMap));
    }

    public override bool IsReadOnly => SiteMap.IsReadOnly;

    public override TValue this[TKey key]
    {
        get => base[key];
        set
        {
            ThrowIfReadOnly();
            base[key] = value;
        }
    }

    public override void Add(KeyValuePair<TKey, TValue> item)
    {
        ThrowIfReadOnly();
        base.Add(item);
    }

    public override void Add(TKey key, TValue value)
    {
        ThrowIfReadOnly();
        base.Add(key, value);
    }

    public override void AddRange(IDictionary<TKey, TValue> items)
    {
        ThrowIfReadOnly();
        base.AddRange(items);
    }

    public override void Clear()
    {
        ThrowIfReadOnly();
        base.Clear();
    }

    protected override void Insert(TKey key, TValue value, bool add)
    {
        ThrowIfReadOnly();
        base.Insert(key, value, add);
    }

    public override bool Remove(KeyValuePair<TKey, TValue> item)
    {
        ThrowIfReadOnly();
        return base.Remove(item);
    }

    public override bool Remove(TKey key)
    {
        ThrowIfReadOnly();
        return base.Remove(key);
    }

    public virtual void CopyTo(IDictionary<TKey, TValue> destination)
    {
        foreach (var item in Dictionary)
        {
            // Use null-forgiving to satisfy analyzer - reference types used as designed.
            var keyType = item.Key!.GetType();
            var valueType = item.Value!.GetType();
            var keyIsPointer = keyType.IsPointer;
            var valueIsPointer = valueType.IsPointer;
            if (!keyIsPointer && !valueIsPointer)
            {
                destination.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value));
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
