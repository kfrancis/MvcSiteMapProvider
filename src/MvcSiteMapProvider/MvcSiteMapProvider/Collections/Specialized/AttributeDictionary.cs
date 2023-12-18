using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web.Script.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace MvcSiteMapProvider.Collections.Specialized
{
    /// <summary>
    /// A specialized dictionary that contains the business logic for handling the attributes collection including
    /// localization of custom attributes.
    /// </summary>
    [ExcludeFromAutoRegistration]
    public class AttributeDictionary
        : CacheableDictionary<string, object>, IAttributeDictionary
    {
        public AttributeDictionary(
            string siteMapNodeKey,
            string memberName,
            ISiteMap siteMap,
            ILocalizationService localizationService,
            IReservedAttributeNameProvider reservedAttributeNameProvider,
            IJsonToDictionaryDeserializer jsonToDictionaryDeserializer,
            ICache cache
            )
            : base(siteMap, cache)
        {
            if (string.IsNullOrEmpty(siteMapNodeKey))
                throw new ArgumentNullException(nameof(siteMapNodeKey));
            if (string.IsNullOrEmpty(memberName))
                throw new ArgumentNullException(nameof(memberName));
            this.siteMapNodeKey = siteMapNodeKey;
            this.memberName = memberName;
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.reservedAttributeNameProvider = reservedAttributeNameProvider ?? throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
            this.jsonToDictionaryDeserializer = jsonToDictionaryDeserializer ?? throw new ArgumentNullException(nameof(jsonToDictionaryDeserializer));
        }

        protected readonly string siteMapNodeKey;
        protected readonly string memberName;
        protected readonly ILocalizationService localizationService;
        protected readonly IReservedAttributeNameProvider reservedAttributeNameProvider;
        protected readonly IJsonToDictionaryDeserializer jsonToDictionaryDeserializer;

        protected override string GetCacheKey()
        {
            return "__ATTRIBUTE_DICTIONARY_" + siteMap.CacheKey + "_" + siteMapNodeKey + "_" + memberName + "_";
        }

        /// <summary>
        /// Adds a new element to the dictionary with the specified key and value. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="key">The key of the new item to add.</param>
        /// <param name="value">The value of the new item to add.</param>
        public override void Add(string key, object value)
        {
            Add(key, value, true);
        }

        /// <summary>
        /// Adds a new element to the dictionary with the values specified in the KeyValuePair. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="item">The KeyValuePair object that contains the key and value to add.</param>
        public override void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value, true);
        }

        /// <summary>
        /// Adds a new element to the dictionary with the values specified in the KeyValuePair. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="item">The KeyValuePair object that contains the key and value to add.</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void Add(KeyValuePair<string, object> item, bool throwIfReservedKey)
        {
            Add(item.Key, item.Value, throwIfReservedKey);
        }

        /// <summary>
        /// Adds a new element to the dictionary with the specified key and value. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void Add(string key, object value, bool throwIfReservedKey)
        {
            if (reservedAttributeNameProvider.IsRegularAttribute(key))
            {
                if (value.GetType().Equals(typeof(string)))
                    value = localizationService.ExtractExplicitResourceKey(key, value.ToString());

                if (!ContainsKey(key))
                    base.Add(key, value);
                else
                    base[key] = value;
            }
            else if (throwIfReservedKey)
            {
                throw new ReservedKeyException(string.Format(Resources.Messages.AttributeKeyReserved, siteMapNodeKey, key, value));
            }
        }

        /// <summary>
        /// Adds the elements from a <see cref="T:System.Collections.Generic.Dictionary{string, object}"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="items">The <see cref="T:System.Collections.Generic.Dictionary{string, object}"/> of items to add.</param>
        public override void AddRange(IDictionary<string, object> items)
        {
            AddRange(items, true);
        }

        /// <summary>
        /// Adds the elements from a <see cref="T:System.Collections.Generic.Dictionary{string, object}"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="items">The <see cref="T:System.Collections.Generic.Dictionary{string, object}"/> of items to add.</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void AddRange(IDictionary<string, object> items, bool throwIfReservedKey)
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value, throwIfReservedKey);
            }
        }

        /// <summary>
        /// Adds the elements from a JSON string representing the attributes. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="jsonString">A JSON string that represents a dictionary of key-value pairs. Example: @"{ ""key-1"": ""value-1""[, ""key-x"": ""value-x""] }".
        /// The value may be a string or primitive type (by leaving off the quotes).</param>
        public void AddRange(string jsonString)
        {
            AddRange(jsonString, true);
        }

        /// <summary>
        /// Adds the elements from a JSON string representing the attributes. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="jsonString">A JSON string that represents a dictionary of key-value pairs. Example: @"{ ""key-1"": ""value-1""[, ""key-x"": ""value-x""] }".
        /// The value may be a string or primitive type (by leaving off the quotes).</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void AddRange(string jsonString, bool throwIfReservedKey)
        {
            var items = jsonToDictionaryDeserializer.Deserialize(jsonString);
            AddRange(items, throwIfReservedKey);
        }

        /// <summary>
        /// Adds the elements from a given <see cref="System.Xml.Linq.XElement"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="xmlNode">The <see cref="System.Xml.Linq.XElement"/> that represents the siteMapNode element in XML.</param>
        public void AddRange(XElement xmlNode)
        {
            AddRange(xmlNode, true);
        }

        /// <summary>
        /// Adds the elements from a given <see cref="System.Xml.Linq.XElement"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="xmlNode">The <see cref="System.Xml.Linq.XElement"/> that represents the siteMapNode element in XML.</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void AddRange(XElement xmlNode, bool throwIfReservedKey)
        {
            foreach (var attribute in xmlNode.Attributes())
            {
                Add(attribute.Name.ToString(), attribute.Value, throwIfReservedKey);
            }
        }

        /// <summary>
        /// Adds the elements from a given <see cref="System.Collections.Specialized.NameValueCollection"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="nameValueCollection">The <see cref="System.Collections.Specialized.NameValueCollection"/> to retrieve the values from.</param>
        public void AddRange(NameValueCollection nameValueCollection)
        {
            AddRange(nameValueCollection, true);
        }

        /// <summary>
        /// Adds the elements from a given <see cref="System.Collections.Specialized.NameValueCollection"/>. If the key exists, the value will be overwritten.
        /// </summary>
        /// <param name="nameValueCollection">The <see cref="System.Collections.Specialized.NameValueCollection"/> to retrieve the values from.</param>
        /// <param name="throwIfReservedKey"><c>true</c> to throw an exception if one of the keys being added is a reserved key name; otherwise, <c>false</c>.</param>
        public void AddRange(NameValueCollection nameValueCollection, bool throwIfReservedKey)
        {
            foreach (string key in nameValueCollection.Keys)
            {
                Add(key, nameValueCollection[key], throwIfReservedKey);
            }
        }

        public override void Clear()
        {
            base.Clear();
            if (!IsReadOnly)
            {
                foreach (var key in Keys)
                {
                    localizationService.RemoveResourceKey(key);
                }
            }
        }

        protected override void Insert(string key, object value, bool add)
        {
            Insert(key, value, add, true);
        }

        protected void Insert(string key, object value, bool add, bool throwIfReservedKey)
        {
            if (reservedAttributeNameProvider.IsRegularAttribute(key))
            {
                if (value.GetType().Equals(typeof(string)))
                    value = localizationService.ExtractExplicitResourceKey(key, value.ToString());
                base.Insert(key, value, add);
            }
            else if (throwIfReservedKey)
            {
                throw new ReservedKeyException(string.Format(Resources.Messages.AttributeKeyReserved, siteMapNodeKey, key, value));
            }
        }

        public override bool Remove(KeyValuePair<string, object> item)
        {
            var removed = base.Remove(item);
            if (removed && !IsReadOnly)
                localizationService.RemoveResourceKey(item.Key);
            return removed;
        }

        public override bool Remove(string key)
        {
            var removed = base.Remove(key);
            if (removed && !IsReadOnly)
                localizationService.RemoveResourceKey(key);
            return removed;
        }

        public override object this[string key]
        {
            get
            {
                var value = base[key];
                return value.GetType().Equals(typeof(string)) ? localizationService.GetResourceString(key, value.ToString(), base.siteMap) : value;
            }
            set
            {
                if (reservedAttributeNameProvider.IsRegularAttribute(key))
                {
                    if (value.GetType().Equals(typeof(string)))
                    {
                        base[key] = localizationService.ExtractExplicitResourceKey(key, value.ToString());
                    }
                    else
                    {
                        base[key] = value;
                    }
                }
                else
                {
                    throw new ReservedKeyException(string.Format(Resources.Messages.AttributeKeyReserved, siteMapNodeKey, key, value));
                }
            }
        }
    }
}