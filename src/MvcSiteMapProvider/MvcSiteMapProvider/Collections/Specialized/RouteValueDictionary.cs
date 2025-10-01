using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Caching;
using MvcSiteMapProvider.DI;
using MvcSiteMapProvider.Resources;
using MvcSiteMapProvider.Web.Script.Serialization;

namespace MvcSiteMapProvider.Collections.Specialized;

/// <summary>
///     Specialized dictionary for providing business logic that manages
///     the behavior of the route values.
/// </summary>
[ExcludeFromAutoRegistration]
public class RouteValueDictionary
    : CacheableDictionary<string, object>, IRouteValueDictionary
{
    private readonly IJsonToDictionaryDeserializer _jsonToDictionaryDeserializer;
    private readonly string _memberName;
    private readonly IReservedAttributeNameProvider _reservedAttributeNameProvider;

    private readonly string _siteMapNodeKey;

    public RouteValueDictionary(
        string siteMapNodeKey,
        string memberName,
        ISiteMap siteMap,
        IReservedAttributeNameProvider reservedAttributeNameProvider,
        IJsonToDictionaryDeserializer jsonToDictionaryDeserializer,
        ICache cache
    ) : base(siteMap, cache)
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
        _reservedAttributeNameProvider = reservedAttributeNameProvider ??
                                         throw new ArgumentNullException(nameof(reservedAttributeNameProvider));
        _jsonToDictionaryDeserializer = jsonToDictionaryDeserializer ??
                                        throw new ArgumentNullException(nameof(jsonToDictionaryDeserializer));

        // An area route value must always exist, so we add it here to ensure it does.
        this["area"] = string.Empty;
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
        if (_reservedAttributeNameProvider.IsRouteAttribute(key))
        {
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
            throw new ReservedKeyException(string.Format(Messages.RouteValueKeyReserved, _siteMapNodeKey, key, value));
        }
    }

    /// <summary>
    ///     Adds the elements from a <see cref="T:System.Collections.Generic.IDictionary{string, object}" />. If the key
    ///     exists, the value will be overwritten.
    /// </summary>
    /// <param name="items">The <see cref="T:System.Collections.Generic.IDictionary{string, object}" /> of items to add.</param>
    public override void AddRange(IDictionary<string, object> items)
    {
        AddRange(items, true);
    }

    /// <summary>
    ///     Adds the elements from a <see cref="T:System.Collections.Generic.IDictionary{string, object}" />. If the key
    ///     exists, the value will be overwritten.
    /// </summary>
    /// <param name="items">The <see cref="T:System.Collections.Generic.IDictionary{string, object}" /> of items to add.</param>
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

    public override object this[string key]
    {
        get => base[key];
        set
        {
            if (_reservedAttributeNameProvider.IsRouteAttribute(key))
            {
                base[key] = value;
            }
            else
            {
                throw new ReservedKeyException(string.Format(Messages.RouteValueKeyReserved, _siteMapNodeKey, key,
                    value));
            }
        }
    }

    /// <summary>
    ///     <b>True</b> if the dictionary contains keys other than "area", "controller", and "action"; otherwise <b>false</b>.
    /// </summary>
    public virtual bool ContainsCustomKeys
    {
        get
        {
            if (Count > 3)
            {
                return true;
            }

            foreach (var key in Keys)
            {
                if (IsCustomKey(key))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Obsolete(
        "Use the overload MatchesRoute(IDictionary<string, object>) instead. This overload will be removed in version 5.")]
    public virtual bool MatchesRoute(IEnumerable<string> actionParameters, IDictionary<string, object> routeValues)
    {
        return MatchesRoute(routeValues);
    }

    public virtual bool MatchesRoute(IDictionary<string, object> routeValues)
    {
        if (routeValues.Count == 0)
        {
            return false;
        }

        foreach (var pair in routeValues)
        {
            if (!MatchesRouteValue(pair.Key, pair.Value))
            {
                return false;
            }
        }

        // Locate any orphan keys (with non-empty values) in the current configuration that were not
        // included in the comparison. We only want to match if all them were considered.
        var remainingList = (from rv in this
                             where !IsEmptyValue(rv.Value)
                             where routeValues.Keys.All(x => x != rv.Key)
                             select rv)
            .ToDictionary(x => x.Key);

        return !remainingList.Any();
    }

    protected override string GetCacheKey()
    {
        return "__ROUTE_VALUE_DICTIONARY_" + SiteMap.CacheKey + "_" + _siteMapNodeKey + "_" + _memberName + "_";
    }

    protected override void Insert(string key, object value, bool add)
    {
        Insert(key, value, add, true);
    }

    private void Insert(string key, object value, bool add, bool throwIfReservedKey)
    {
        if (_reservedAttributeNameProvider.IsRouteAttribute(key))
        {
            base.Insert(key, value, add);
        }
        else if (throwIfReservedKey)
        {
            throw new ReservedKeyException(string.Format(Messages.RouteValueKeyReserved, _siteMapNodeKey, key, value));
        }
    }

    protected virtual bool IsCustomKey(string key)
    {
        return string.IsNullOrEmpty(key) || (key != "area" && key != "controller" && key != "action");
    }

    protected virtual bool MatchesRouteValue(string key, object value)
    {
        if (ValueExists(key))
        {
            if (MatchesValue(key, value))
            {
                return true;
            }
        }
        else
        {
            if (IsEmptyValue(value))
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool MatchesValue(string key, object value)
    {
        return this[key].ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool IsEmptyValue(object? value)
    {
        return value == null ||
               string.IsNullOrEmpty(value.ToString());
    }

    protected virtual bool ValueExists(string key)
    {
        if (!ContainsKey(key))
        {
            return false;
        }

        return !string.IsNullOrEmpty(this[key].ToString());
    }
}
