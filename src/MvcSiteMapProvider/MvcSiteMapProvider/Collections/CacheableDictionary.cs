using MvcSiteMapProvider.Caching;
using System;
using System.Collections.Generic;

#if !NET35

using System.Collections.Specialized;

#endif

namespace MvcSiteMapProvider.Collections
{
    /// <summary>
    /// Generic dictionary that is aware of the request cache and when is in read-only
    /// mode will automatically switch to a writeable request-cached copy of the original dictionary
    /// during any write operation.
    /// </summary>
    public class CacheableDictionary<TKey, TValue>
        : LockableDictionary<TKey, TValue>
    {
        protected readonly ICache cache;

        protected readonly Guid instanceId = Guid.NewGuid();

        public CacheableDictionary(
                            ISiteMap siteMap,
            ICache cache
            )
            : base(siteMap)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public override int Count
        {
            get { return ReadOperationDictionary.Count; }
        }

        public override ICollection<TKey> Keys
        {
            get { return ReadOperationDictionary.Keys; }
        }

        public override ICollection<TValue> Values
        {
            get { return ReadOperationDictionary.Values; }
        }

        /// <summary>
        /// Override this property and set it to false to disable all caching operations.
        /// </summary>
        protected virtual bool CachingEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a dictionary object that can be used to perform a read operation.
        /// </summary>
        protected virtual IDictionary<TKey, TValue> ReadOperationDictionary
        {
            get
            {
                IDictionary<TKey, TValue> result;
                if (CachingEnabled)
                {
                    var key = GetCacheKey();
                    result = cache.GetValue<IDictionary<TKey, TValue>>(key);
                    if (result == null)
                    {
                        // Request is not cached, return base dictionary
                        result = base.Dictionary;
                    }
                }
                else
                {
                    result = base.Dictionary;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets a dictionary object that can be used to perform a write operation.
        /// </summary>
        protected virtual IDictionary<TKey, TValue> WriteOperationDictionary
        {
            get
            {
                IDictionary<TKey, TValue> result;
                if (IsReadOnly && CachingEnabled)
                {
                    var key = GetCacheKey();
                    result = cache.GetValue<IDictionary<TKey, TValue>>(key);
                    if (result == null)
                    {
                        // This is the first write operation request in read-only mode,
                        // we need to create a new dictionary and cache it
                        // with a copy of the current values.
                        result = new Dictionary<TKey, TValue>();
                        base.CopyTo(result);
                        cache.SetValue(key, result);
                    }
                }
                else
                {
                    result = base.Dictionary;
                }
                return result;
            }
        }

        public override TValue this[TKey key]
        {
            get { return ReadOperationDictionary[key]; }
            set { WriteOperationDictionary[key] = value; }
        }

        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            WriteOperationDictionary.Add(item);
        }

        public override void Add(TKey key, TValue value)
        {
            WriteOperationDictionary.Add(key, value);
        }

        public override void AddRange(IDictionary<TKey, TValue> items)
        {
            foreach (var item in items)
            {
                WriteOperationDictionary.Add(item);
            }
        }

        public override void Clear()
        {
            WriteOperationDictionary.Clear();
        }

        public override bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ReadOperationDictionary.Contains(item);
        }

        public override bool ContainsKey(TKey key)
        {
            return ReadOperationDictionary.ContainsKey(key);
        }

        public override void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ReadOperationDictionary.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj)
        {
            return ReadOperationDictionary.Equals(obj);
        }

        public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ReadOperationDictionary.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return ReadOperationDictionary.GetHashCode();
        }

        public override bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return WriteOperationDictionary.Remove(item);
        }

        public override bool Remove(TKey key)
        {
            return WriteOperationDictionary.Remove(key);
        }

        public override string ToString()
        {
            return ReadOperationDictionary.ToString();
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return ReadOperationDictionary.TryGetValue(key, out value);
        }

        protected virtual string GetCacheKey()
        {
            return "__CACHEABLE_DICTIONARY_" + instanceId.ToString();
        }

        protected override void Insert(TKey key, TValue value, bool add)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (ReadOperationDictionary.TryGetValue(key, out TValue item))
            {
                if (add) throw new ArgumentException(Resources.Messages.DictionaryAlreadyContainsKey);
                if (Equals(item, value)) return;
                WriteOperationDictionary[key] = value;
                #if !NET35
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
                #endif
            }
            else
            {
                WriteOperationDictionary[key] = value;
                #if !NET35
                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
                #endif
            }
        }
    }
}