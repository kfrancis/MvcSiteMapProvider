using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// Specialized dictionary for storing metadata about a specific instance of a MvcSiteMapProvider Html Helper.
    /// </summary>
    public class SourceMetadataDictionary
        : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private readonly Dictionary<string, object> _dictionary;

        public SourceMetadataDictionary()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public SourceMetadataDictionary(IDictionary<string, object> dictionary)
        {
            _dictionary = new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
        }

        public SourceMetadataDictionary(object values)
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            AddValues(values);
        }

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        private void AddValues(object values)
        {
            if (values != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    var value = descriptor.GetValue(values);
                    Add(descriptor.Name, value);
                }
            }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(object value)
        {
            return _dictionary.ContainsValue(value);
        }

        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).Add(item);
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Remove(item);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public object this[string key]
        {
            get
            {
                TryGetValue(key, out var obj2);
                return obj2;
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        public Dictionary<string, object>.KeyCollection Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<string, object>>)_dictionary).IsReadOnly;
            }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return _dictionary.Keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        public Dictionary<string, object>.ValueCollection Values
        {
            get
            {
                return _dictionary.Values;
            }
        }
    }
}
