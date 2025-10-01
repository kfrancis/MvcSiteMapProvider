using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Web.Script.Serialization;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     A specialized dictionary that contains the business logic for handling the attributes collection including
///     localization of custom attributes.
/// </summary>
[ExcludeFromAutoRegistration]
public class AttributeDictionary
    : CacheableDictionary<string, object>, IAttributeDictionary
{
    private readonly IJsonToDictionaryDeserializer _jsonToDictionaryDeserializer;
    private readonly ILocalizationService _localizationService;
    private readonly string _memberName;
    private readonly IReservedAttributeNameProvider _reservedAttributeNameProvider;

    private readonly string _siteMapNodeKey;

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
        {
            throw new ArgumentNullException(nameof(siteMapNodeKey));
        }

        if (string.IsNullOrEmpty(memberName))
        {
            throw new ArgumentNullException(nameof(memberName));
        }

        _siteMapNodeKey = siteMapNodeKey;
        _memberName = memberName;
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _reservedAttributeNameProvider = reservedAttributeNameProvider ??
                                         throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _jsonToDictionaryDeserializer = jsonToDictionaryDeserializer ??
                                        throw new ArgumentNullException(nameof(jsonToDictionaryDeserializer));
    }

    /// <summary>
    ///     Adds a new element to the dictionary with the specified key and value. If the key exists, the value will be
    ///     overwritten.
    /// </summary>
    /// <param name="key">The key of the new item to add.</param>
    /// <param name="value">The value of the new item to add.</param>
    public override void Add(string key, object value)
    {
        Add(key, value, true);
    }

    /// <summary>
    ///     Adds a new element to the dictionary with the values specified in the KeyValuePair. If the key exists, the value
    ///     will be overwritten.
    /// </summary>
    /// <param name="item">The KeyValuePair object that contains the key and value to add.</param>
    public override void Add(KeyValuePair<string, object> item)
    {
        Add(item.Key, item.Value, true);
    }

    /// <summary>
    ///     Adds a new element to the dictionary with the values specified in the KeyValuePair. If the key exists, the value
    ///     will be overwritten.
    /// </summary>
    /// <param name="item">The KeyValuePair object that contains the key and value to add.</param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
    public void Add(KeyValuePair<string, object> item, bool throwIfReservedKey)
    {
        Add(item.Key, item.Value, throwIfReservedKey);
    }

    /// <summary>
    ///     Adds a new element to the dictionary with the specified key and value. If the key exists, the value will be
    ///     overwritten.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
    public void Add(string key, object value, bool throwIfReservedKey)
    {
        if (_reservedAttributeNameProvider.IsRegularAttribute(key))
        {
            if (value is string)
            {
                value = _localizationService.ExtractExplicitResourceKey(key, value.ToString());
            }

            if (!ContainsKey(key))
            {
                base.Add(key, value);
            }
            else
            {
                base[key] = value;
            }
        }
        else if (throwIfReservedKey)
        {
            throw new ReservedKeyException(string.Format(Messages.AttributeKeyReserved, _siteMapNodeKey, key, value));
        }
    }

    /// <summary>
    ///     Adds the elements from a <see cref="T:System.Collections.Generic.Dictionary{string, object}" />. If the key exists,
    ///     the value will be overwritten.
    /// </summary>
    /// <param name="items">The <see cref="T:System.Collections.Generic.Dictionary{string, object}" /> of items to add.</param>
    public override void AddRange(IDictionary<string, object> items)
    {
        AddRange(items, true);
    }

    /// <summary>
    ///     Adds the elements from a <see cref="T:System.Collections.Generic.Dictionary{string, object}" />. If the key exists,
    ///     the value will be overwritten.
    /// </summary>
    /// <param name="items">The <see cref="T:System.Collections.Generic.Dictionary{string, object}" /> of items to add.</param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
    public void AddRange(IDictionary<string, object> items, bool throwIfReservedKey)
    {
        foreach (var item in items)
        {
            Add(item.Key, item.Value, throwIfReservedKey);
        }
    }

    /// <summary>
    ///     Adds the elements from a JSON string representing the attributes. If the key exists, the value will be overwritten.
    /// </summary>
    /// <param name="jsonString">
    ///     A JSON string that represents a dictionary of key-value pairs. Example: @"{ ""key-1"": ""value-1""[, ""key-x"":
    ///     ""value-x""] }".
    ///     The value may be a string or primitive type (by leaving off the quotes).
    /// </param>
    public void AddRange(string jsonString)
    {
        AddRange(jsonString, true);
    }

    /// <summary>
    ///     Adds the elements from a JSON string representing the attributes. If the key exists, the value will be overwritten.
    /// </summary>
    /// <param name="jsonString">
    ///     A JSON string that represents a dictionary of key-value pairs. Example: @"{ ""key-1"": ""value-1""[, ""key-x"":
    ///     ""value-x""] }".
    ///     The value may be a string or primitive type (by leaving off the quotes).
    /// </param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
    public void AddRange(string jsonString, bool throwIfReservedKey)
    {
        var items = _jsonToDictionaryDeserializer.Deserialize(jsonString);
        AddRange(items, throwIfReservedKey);
    }

    /// <summary>
    ///     Adds the elements from a given <see cref="System.Xml.Linq.XElement" />. If the key exists, the value will be
    ///     overwritten.
    /// </summary>
    /// <param name="xmlNode">The <see cref="System.Xml.Linq.XElement" /> that represents the siteMapNode element in XML.</param>
    public void AddRange(XElement xmlNode)
    {
        AddRange(xmlNode, true);
    }

    /// <summary>
    ///     Adds the elements from a given <see cref="System.Xml.Linq.XElement" />. If the key exists, the value will be
    ///     overwritten.
    /// </summary>
    /// <param name="xmlNode">The <see cref="System.Xml.Linq.XElement" /> that represents the siteMapNode element in XML.</param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
    public void AddRange(XElement xmlNode, bool throwIfReservedKey)
    {
        foreach (var attribute in xmlNode.Attributes())
        {
            Add(attribute.Name.ToString(), attribute.Value, throwIfReservedKey);
        }
    }

    /// <summary>
    ///     Adds the elements from a given <see cref="System.Collections.Specialized.NameValueCollection" />. If the key
    ///     exists, the value will be overwritten.
    /// </summary>
    /// <param name="nameValueCollection">
    ///     The <see cref="System.Collections.Specialized.NameValueCollection" /> to retrieve the
    ///     values from.
    /// </param>
    public void AddRange(NameValueCollection nameValueCollection)
    {
        AddRange(nameValueCollection, true);
    }

    /// <summary>
    ///     Adds the elements from a given <see cref="System.Collections.Specialized.NameValueCollection" />. If the key
    ///     exists, the value will be overwritten.
    /// </summary>
    /// <param name="nameValueCollection">
    ///     The <see cref="System.Collections.Specialized.NameValueCollection" /> to retrieve the
    ///     values from.
    /// </param>
    /// <param name="throwIfReservedKey">
    ///     <c>true</c> to throw an exception if one of the keys being added is a reserved key
    ///     name; otherwise, <c>false</c>.
    /// </param>
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
        if (IsReadOnly)
        {
            return;
        }

        foreach (var key in Keys)
        {
            _localizationService.RemoveResourceKey(key);
        }
    }

    public override bool Remove(KeyValuePair<string, object> item)
    {
        var removed = base.Remove(item);
        if (removed && !IsReadOnly)
        {
            _localizationService.RemoveResourceKey(item.Key);
        }

        return removed;
    }

    public override bool Remove(string key)
    {
        var removed = base.Remove(key);
        if (removed && !IsReadOnly)
        {
            _localizationService.RemoveResourceKey(key);
        }

        return removed;
    }

    public override object this[string key]
    {
        get
        {
            var value = base[key];
            return value is string ? _localizationService.GetResourceString(key, value.ToString(), SiteMap) : value;
        }
        set
        {
            if (_reservedAttributeNameProvider.IsRegularAttribute(key))
            {
                if (value is string)
                {
                    base[key] = _localizationService.ExtractExplicitResourceKey(key, value.ToString());
                }
                else
                {
                    base[key] = value;
                }
            }
            else
            {
                throw new ReservedKeyException(
                    string.Format(Messages.AttributeKeyReserved, _siteMapNodeKey, key, value));
            }
        }
    }

    protected override string GetCacheKey()
    {
        return "__ATTRIBUTE_DICTIONARY_" + SiteMap.CacheKey + "_" + _siteMapNodeKey + "_" + _memberName + "_";
    }

    protected override void Insert(string key, object value, bool add)
    {
        Insert(key, value, add, true);
    }

    private void Insert(string key, object value, bool add, bool throwIfReservedKey)
    {
        if (_reservedAttributeNameProvider.IsRegularAttribute(key))
        {
            if (value is string)
            {
                value = _localizationService.ExtractExplicitResourceKey(key, value.ToString());
            }

            base.Insert(key, value, add);
        }
        else if (throwIfReservedKey)
        {
            throw new ReservedKeyException(string.Format(Messages.AttributeKeyReserved, _siteMapNodeKey, key, value));
        }
    }
}
