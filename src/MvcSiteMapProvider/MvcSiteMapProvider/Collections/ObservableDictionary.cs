using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MvcSiteMapProvider.Resources;
#if !NET35
using System.Collections.Specialized;
#endif

// Source: http://blogs.microsoft.co.il/blogs/shimmy/archive/2010/12/26/observabledictionary-lt-tkey-tvalue-gt-c.aspx

namespace MvcSiteMapProvider.Collections;

public class ObservableDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>,
#if !NET35
        INotifyCollectionChanged,
#endif
        INotifyPropertyChanged
{
    private const string CountString = "Count";
    private const string IndexerName = "Item[]";
    private const string KeysName = "Keys";
    private const string ValuesName = "Values";

    protected ObservableDictionary()
    {
        Dictionary = new Dictionary<TKey, TValue>();
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        Dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        Dictionary = new Dictionary<TKey, TValue>(comparer);
    }

    public ObservableDictionary(int capacity)
    {
        Dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
    }

    public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
        Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
    }

    protected IDictionary<TKey, TValue> Dictionary { get; private set; }

    public virtual void Add(TKey key, TValue value)
    {
        Insert(key, value, true);
    }

    public virtual bool ContainsKey(TKey key)
    {
        return Dictionary.ContainsKey(key);
    }

    public virtual ICollection<TKey> Keys => Dictionary.Keys;

    public virtual bool Remove(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        //Dictionary.TryGetValue(key, out var value);
        var removed = Dictionary.Remove(key);
        if (removed)
        {
            //OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
            OnCollectionChanged();
        }

        return removed;
    }

    public virtual bool TryGetValue(TKey key, out TValue value)
    {
        return Dictionary.TryGetValue(key, out value);
    }

    public virtual ICollection<TValue> Values => Dictionary.Values;

    public virtual TValue this[TKey key]
    {
        get => Dictionary[key];
        set => Insert(key, value, false);
    }

    public virtual void Add(KeyValuePair<TKey, TValue> item)
    {
        Insert(item.Key, item.Value, true);
    }

    public virtual void Clear()
    {
        if (Dictionary.Count > 0)
        {
            Dictionary.Clear();
            OnCollectionChanged();
        }
    }

    public virtual bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return Dictionary.Contains(item);
    }

    public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        Dictionary.CopyTo(array, arrayIndex);
    }

    public virtual int Count => Dictionary.Count;

    public virtual bool IsReadOnly => Dictionary.IsReadOnly;

    public virtual bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Dictionary).GetEnumerator();
    }

#if !NET35
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
#endif

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void AddRange(IDictionary<TKey, TValue> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (items.Count > 0)
        {
            if (Dictionary.Count > 0)
            {
                if (items.Keys.Any(k => Dictionary.ContainsKey(k)))
                {
                    throw new ArgumentException(Messages.DictionaryAlreadyContainsKey);
                }

                foreach (var item in items)
                {
                    Dictionary.Add(item);
                }
            }
            else
            {
                Dictionary = new Dictionary<TKey, TValue>(items);
            }
#if !NET35
            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
#endif
        }
    }

    protected virtual void Insert(TKey key, TValue value, bool add)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (Dictionary.TryGetValue(key, out var item))
        {
            if (add)
            {
                throw new ArgumentException(Messages.DictionaryAlreadyContainsKey);
            }

            if (Equals(item, value))
            {
                return;
            }

            Dictionary[key] = value;
#if !NET35
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value),
                new KeyValuePair<TKey, TValue>(key, item));
#endif
        }
        else
        {
            Dictionary[key] = value;
#if !NET35
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
#endif
        }
    }

    protected virtual void OnPropertyChanged()
    {
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(KeysName);
        OnPropertyChanged(ValuesName);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    protected void OnCollectionChanged()
    {
        OnPropertyChanged();
#if !NET35
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
#endif
    }

#if !NET35
    protected void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem,
        KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }
#endif
}
