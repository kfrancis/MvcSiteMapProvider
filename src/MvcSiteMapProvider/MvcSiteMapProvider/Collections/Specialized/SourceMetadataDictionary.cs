using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     Specialized dictionary for storing metadata about a specific instance of a MvcSiteMapProvider Html Helper.
/// </summary>
public class SourceMetadataDictionary
    : IDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _dictionary;

    public SourceMetadataDictionary()
    {
        _dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public SourceMetadataDictionary(IDictionary<string, object?> dictionary)
    {
        _dictionary = new Dictionary<string, object?>(dictionary, StringComparer.OrdinalIgnoreCase);
    }

    public SourceMetadataDictionary(object values)
    {
        _dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        AddValues(values);
    }

    public Dictionary<string, object?>.KeyCollection Keys => _dictionary.Keys;

    public Dictionary<string, object?>.ValueCollection Values => _dictionary.Values;

    public void Add(string key, object? value)
    {
        _dictionary.Add(key, value);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _dictionary.Remove(key);
    }

    void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item)
    {
        ((ICollection<KeyValuePair<string, object?>>)_dictionary).Add(item);
    }

    bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
    {
        return _dictionary.Contains(item);
    }

    void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, object?>>)_dictionary).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item)
    {
        return ((ICollection<KeyValuePair<string, object?>>)_dictionary).Remove(item);
    }

    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryGetValue(string key, out object? value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public int Count => _dictionary.Count;

    public object? this[string key]
    {
        get
        {
            TryGetValue(key, out var obj2);
            return obj2;
        }
        set => _dictionary[key] = value;
    }

    bool ICollection<KeyValuePair<string, object?>>.IsReadOnly =>
        ((ICollection<KeyValuePair<string, object?>>)_dictionary).IsReadOnly;

    ICollection<string> IDictionary<string, object?>.Keys => _dictionary.Keys;

    ICollection<object?> IDictionary<string, object?>.Values => _dictionary.Values;

    private void AddValues(object? values)
    {
        if (values == null)
        {
            return;
        }

        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
        {
            var value = descriptor.GetValue(values);
            Add(descriptor.Name, value);
        }
    }

    public bool ContainsValue(object value)
    {
        return _dictionary.ContainsValue(value);
    }

    private Dictionary<string, object?>.Enumerator GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }
}
