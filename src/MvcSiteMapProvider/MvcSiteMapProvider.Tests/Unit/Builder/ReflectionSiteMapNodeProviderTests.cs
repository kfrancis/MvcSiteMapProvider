using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Reflection;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Builder
{
    // NOTE: The method under test is protected. We derive a test double to expose it.
    public class ReflectionSiteMapNodeProviderTests
    {
        [Test]
        public void ReturnsNull_When_CacheKey_DoesNotMatch()
        {
            var attribute = new MvcSiteMapNodeAttribute { Title = "T", SiteMapCacheKey = "A" };
            var helper = new StubSiteMapNodeHelper("B");
            var provider = new TestableReflectionSiteMapNodeProvider();
            var result = provider.Expose(attribute, typeof(BasicController), helper);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Discovers_Index_Action_When_MethodInfo_Null()
        {
            var attribute = new MvcSiteMapNodeAttribute { Title = "Index Title" };
            var helper = new StubSiteMapNodeHelper(null);
            var provider = new TestableReflectionSiteMapNodeProvider();
            var result = provider.Expose(attribute, typeof(BasicController), helper);
            // A node should be created when methodInfo is null but an Index action is discovered.
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Node.Title, Is.EqualTo("Index Title"));
            Assert.That(result.Node.RouteValues["action"].ToString(), Is.EqualTo("Index"));
        }

        [Test]
        public void Uses_ActionNameAttribute_For_Action_Value()
        {
            var method = typeof(BasicController).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.Name == "Renamed");
            var attribute = new MvcSiteMapNodeAttribute { Title = "Custom" };
            var helper = new StubSiteMapNodeHelper(null);
            var provider = new TestableReflectionSiteMapNodeProvider();
            var result = provider.Expose(attribute, typeof(BasicController), method, helper);
            // A node is created; ensure ActionNameAttribute is honored.
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Node.RouteValues["action"].ToString(), Is.EqualTo("RenamedAction"));
        }

        [Test]
        public void Populates_Basic_Properties_From_Attribute()
        {
            var attribute = new MvcSiteMapNodeAttribute
            {
                Title = "My Title",
                Description = "Desc",
                HttpMethod = "POST",
                Clickable = false,
                Url = "/custom/url"
            };
            var helper = new StubSiteMapNodeHelper(null);
            var provider = new TestableReflectionSiteMapNodeProvider();
            var method = typeof(BasicController).GetMethod("Index", new Type[0]);
            var result = provider.Expose(attribute, typeof(BasicController), method, helper);
            Assert.That(result.Node.Title, Is.EqualTo("My Title"));
            Assert.That(result.Node.Description, Is.EqualTo("Desc"));
            Assert.That(result.Node.HttpMethod, Is.EqualTo("POST"));
            Assert.That(result.Node.Url, Is.EqualTo("/custom/url"));
            Assert.That(result.Node.Clickable, Is.False);
        }

        #region Test Controller Types

        public class BasicController : Controller
        {
            public ActionResult Index() { return new EmptyResult(); }
            public ActionResult Index(int id) { return new EmptyResult(); }

            [ActionName("RenamedAction")]
            public ActionResult Renamed() { return new EmptyResult(); }
        }

        #endregion

        private class TestableReflectionSiteMapNodeProvider : ReflectionSiteMapNodeProvider
        {
            public TestableReflectionSiteMapNodeProvider() : base(Array.Empty<string>(), Array.Empty<string>(),
                new DummyAttributeAssemblyProviderFactory(), new DummyDefinitionProvider())
            {
            }

            public ISiteMapNodeToParentRelation Expose(IMvcSiteMapNodeAttribute attribute, Type type,
                MethodInfo methodInfo, ISiteMapNodeHelper helper)
            {
                return GetSiteMapNodeFromMvcSiteMapNodeAttribute(attribute, type, methodInfo, helper);
            }

            public ISiteMapNodeToParentRelation Expose(IMvcSiteMapNodeAttribute attribute, Type type,
                ISiteMapNodeHelper helper)
            {
                return GetSiteMapNodeFromMvcSiteMapNodeAttribute(attribute, type, null, helper);
            }
        }
    }

    #region Minimal Test Doubles

    internal class StubSiteMapNodeHelper : ISiteMapNodeHelper
    {
        public StubSiteMapNodeHelper(string cacheKey)
        {
            this.SiteMapCacheKey = cacheKey;
            ReservedAttributeNames = new AllowAllReservedAttributeNames();
        }

        public IReservedAttributeNameProvider ReservedAttributeNames { get; }
        public string SiteMapCacheKey { get; }

        public string CreateNodeKey(string parentKey, string key, string url, string title, string area,
            string controller, string action, string httpMethod, bool clickable)
        {
            return Guid.NewGuid().ToString("N");
        }

        public ISiteMapNodeToParentRelation CreateNode(string key, string parentKey, string sourceName)
        {
            return CreateNode(key, parentKey, sourceName, null);
        }

        public ISiteMapNodeToParentRelation CreateNode(string key, string parentKey, string sourceName,
            string implicitResourceKey)
        {
            return new StubRelation(parentKey, new StubNode(key), sourceName);
        }

        public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node)
        {
            return Array.Empty<ISiteMapNodeToParentRelation>();
        }

        public IEnumerable<ISiteMapNodeToParentRelation> CreateDynamicNodes(ISiteMapNodeToParentRelation node,
            string defaultParentKey)
        {
            return Array.Empty<ISiteMapNodeToParentRelation>();
        }

        public ICultureContext CreateCultureContext(string cultureName, string uiCultureName) { return null; }
        public ICultureContext CreateCultureContext(CultureInfo culture, CultureInfo uiCulture) { return null; }
        public ICultureContext CreateInvariantCultureContext() { return null; }
        public ICultureContext CultureContext { get { return null; } }
    }

    internal class AllowAllReservedAttributeNames : IReservedAttributeNameProvider
    {
        public bool IsRegularAttribute(string attributeName) { return true; }
        public bool IsRouteAttribute(string attributeName) { return true; }
    }

    internal class StubRelation : ISiteMapNodeToParentRelation
    {
        public StubRelation(string parentKey, ISiteMapNode node, string source)
        {
            ParentKey = parentKey;
            Node = node;
            SourceName = source;
        }

        public string ParentKey { get; }
        public ISiteMapNode Node { get; }
        public string SourceName { get; }
    }

    internal class StubNode : ISiteMapNode
    {
        public StubNode(string key)
        {
            Key = key;
            Attributes = new StubAttributeDictionary();
            Roles = new StubRoleCollection();
            RouteValues = new StubRouteValueDictionary();
            PreservedRouteParameters = new StubPreservedParamCollection();
            MetaRobotsValues = new StubMetaRobots();
        }

        public string Key { get; }
        public bool IsDynamic { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public ISiteMapNode ParentNode { get { return null; } }
        public ISiteMapNodeCollection ChildNodes { get { return null; } }
        public ISiteMapNodeCollection Descendants { get { return null; } }
        public ISiteMapNodeCollection Ancestors { get { return null; } }
        public bool IsDescendantOf(ISiteMapNode node) { return false; } 
        public ISiteMapNode NextSibling { get { return null; } }
        public ISiteMapNode PreviousSibling { get { return null; } }
        public ISiteMapNode RootNode { get { return null; } }
        public bool IsInCurrentPath() { return false; }
        public bool HasChildNodes { get { return false; } }
        public int GetNodeLevel() { return 0; }
        public ISiteMap SiteMap { get { return null; } }
        public int Order { get; set; }
        public bool IsAccessibleToUser() { return true; }
        public string HttpMethod { get; set; }
        public string ResourceKey { get { return string.Empty; } }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TargetFrame { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlProtocol { get; set; }
        public string ImageUrlHostName { get; set; }
        public IAttributeDictionary Attributes { get; }
        public IRoleCollection Roles { get; }
        public DateTime LastModifiedDate { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public UpdatePriority UpdatePriority { get; set; }
        public string VisibilityProvider { get; set; }
        public bool IsVisible(IDictionary<string, object?> sourceMetadata) { return true; }
        public string DynamicNodeProvider { get; set; }
        public IEnumerable<DynamicNode> GetDynamicNodeCollection() { return Array.Empty<DynamicNode>(); }
        public bool HasDynamicNodeProvider { get { return false; } }
        public bool Clickable { get; set; }
        public string UrlResolver { get; set; }
        public string Url { get; set; }
        public string UnresolvedUrl { get { return Url; } }
        public string ResolvedUrl { get { return Url; } }
        public bool CacheResolvedUrl { get; set; }
        public void ResolveUrl() { }
        public bool IncludeAmbientValuesInUrl { get; set; }
        public string Protocol { get; set; }
        public string HostName { get; set; }
        public bool HasAbsoluteUrl() { return false; }
        public bool HasExternalUrl(HttpContextBase httpContext) { return false; }
        public string CanonicalKey { get; set; }
        public string CanonicalUrl { get; set; }
        public string CanonicalUrlProtocol { get; set; }
        public string CanonicalUrlHostName { get; set; }
        public IMetaRobotsValueCollection MetaRobotsValues { get; }
        public string GetMetaRobotsContentString() { return string.Empty; }
        public bool HasNoIndexAndNoFollow { get { return false; } }
        public string Route { get; set; }
        public IRouteValueDictionary RouteValues { get; }
        public IPreservedRouteParameterCollection PreservedRouteParameters { get; }
        public RouteData? GetRouteData(HttpContextBase httpContext) { return null; }
        public bool MatchesRoute(IDictionary<string, object> routeValues) { return false; }

        public string Area
        {
            get { return RouteValues.ContainsKey("area") ? Convert.ToString(RouteValues["area"]) : string.Empty; }
            set { RouteValues["area"] = value; }
        }

        public string Controller
        {
            get
            {
                return RouteValues.ContainsKey("controller")
                    ? Convert.ToString(RouteValues["controller"])
                    : string.Empty;
            }
            set { RouteValues["controller"] = value; }
        }

        public string Action
        {
            get { return RouteValues.ContainsKey("action") ? Convert.ToString(RouteValues["action"]) : string.Empty; }
            set { RouteValues["action"] = value; }
        }

        public void CopyTo(ISiteMapNode node) { }
        public bool Equals(ISiteMapNode other) { return other != null && other.Key == Key; }
    }

    internal class StubAttributeDictionary : IAttributeDictionary
    {
        private readonly Dictionary<string, object> _data = new();
        public object this[string key] { get { return _data[key]; } set { _data[key] = value; } }
        public ICollection<string> Keys { get { return _data.Keys; } }
        public ICollection<object> Values { get { return _data.Values; } }
        public int Count { get { return _data.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(string key, object value) { _data[key] = value; }
        public void Add(KeyValuePair<string, object> item) { _data[item.Key] = item.Value; }
        public void Clear() { _data.Clear(); }
        public bool Contains(KeyValuePair<string, object> item) { return _data.ContainsKey(item.Key); }
        public bool ContainsKey(string key) { return _data.ContainsKey(key); }
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return _data.GetEnumerator(); }
        public bool Remove(string key) { return _data.Remove(key); }
        public bool Remove(KeyValuePair<string, object> item) { return _data.Remove(item.Key); }
        public bool TryGetValue(string key, out object value) { return _data.TryGetValue(key, out value); }

        IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }

        // Extended
        public void Add(string key, object value, bool throwIfReservedKey) { Add(key, value); }
        public void Add(KeyValuePair<string, object> item, bool throwIfReservedKey) { Add(item); }

        public void AddRange(IDictionary<string, object> items)
        {
            foreach (var kv in items)
            {
                _data[kv.Key] = kv.Value;
            }
        }

        public void AddRange(IDictionary<string, object> items, bool throwIfReservedKey) { AddRange(items); }
        public void AddRange(string jsonString) { }
        public void AddRange(string jsonString, bool throwIfReservedKey) { }
        public void AddRange(XElement xmlNode) { }
        public void AddRange(XElement xmlNode, bool throwIfReservedKey) { }
        public void AddRange(NameValueCollection nameValueCollection) { }
        public void AddRange(NameValueCollection nameValueCollection, bool throwIfReservedKey) { }

        public void CopyTo(IDictionary<string, object> destination)
        {
        }

        public void CopyTo(IAttributeDictionary destination)
        {
            foreach (var kv in _data)
            {
                destination[kv.Key] = kv.Value;
            }
        }
    }

    internal class StubRouteValueDictionary : IRouteValueDictionary
    {
        private readonly Dictionary<string, object> _data = new();
        public object this[string key] { get { return _data[key]; } set { _data[key] = value; } }
        public ICollection<string> Keys { get { return _data.Keys; } }
        public ICollection<object> Values { get { return _data.Values; } }
        public int Count { get { return _data.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(string key, object value) { _data[key] = value; }
        public void Add(KeyValuePair<string, object> item) { _data[item.Key] = item.Value; }
        public void Clear() { _data.Clear(); }
        public bool Contains(KeyValuePair<string, object> item) { return _data.ContainsKey(item.Key); }
        public bool ContainsKey(string key) { return _data.ContainsKey(key); }
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) { }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() { return _data.GetEnumerator(); }
        public bool Remove(string key) { return _data.Remove(key); }
        public bool Remove(KeyValuePair<string, object> item) { return _data.Remove(item.Key); }
        public bool TryGetValue(string key, out object value) { return _data.TryGetValue(key, out value); }

        IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }

        // Extended
        public void Add(string key, object value, bool throwIfReservedKey) { Add(key, value); }
        public void Add(KeyValuePair<string, object> item, bool throwIfReservedKey) { Add(item); }

        public void AddRange(IDictionary<string, object> items)
        {
            foreach (var kv in items)
            {
                _data[kv.Key] = kv.Value;
            }
        }

        public void AddRange(IDictionary<string, object> items, bool throwIfReservedKey) { AddRange(items); }
        public void AddRange(string jsonString) { }
        public void AddRange(string jsonString, bool throwIfReservedKey) { }
        public void AddRange(XElement xmlNode) { }
        public void AddRange(XElement xmlNode, bool throwIfReservedKey) { }
        public void AddRange(NameValueCollection nameValueCollection) { }
        public void AddRange(NameValueCollection nameValueCollection, bool throwIfReservedKey) { }

        public bool ContainsCustomKeys
        {
            get { return _data.Keys.Any(k => k != "area" && k != "controller" && k != "action"); }
        }

        public bool MatchesRoute(IDictionary<string, object> routeValues) { return false; }

        public bool MatchesRoute(IEnumerable<string> actionParameters, IDictionary<string, object> routeValues)
        {
            return false;
        }

        public void CopyTo(IDictionary<string, object> destination)
        {
            foreach (var kv in _data)
            {
                destination[kv.Key] = kv.Value;
            }
        }
    }

    internal class StubPreservedParamCollection : IPreservedRouteParameterCollection
    {
        private readonly List<string> _data = new();
        public string this[int index] { get { return _data[index]; } set { _data[index] = value; } }
        public int Count { get { return _data.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(string item) { _data.Add(item); }
        public void Clear() { _data.Clear(); }
        public bool Contains(string item) { return _data.Contains(item); }
        public void CopyTo(string[] array, int arrayIndex) { }
        public IEnumerator<string> GetEnumerator() { return _data.GetEnumerator(); }
        public int IndexOf(string item) { return _data.IndexOf(item); }
        public void Insert(int index, string item) { _data.Insert(index, item); }
        public bool Remove(string item) { return _data.Remove(item); }
        public void RemoveAt(int index) { _data.RemoveAt(index); }
        IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }
        public void AddRange(string stringToSplit, char[] separator) { }
        public void AddRange(IEnumerable<string> collection) { }

        public void CopyTo(IList<string> destination)
        {
            foreach (var s in _data)
            {
                destination.Add(s);
            }
        }
    }

    internal class StubRoleCollection : IRoleCollection
    {
        private readonly List<string> _data = new();
        public string this[int index] { get { return _data[index]; } set { _data[index] = value; } }
        public int Count { get { return _data.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(string item) { _data.Add(item); }
        public void Clear() { _data.Clear(); }
        public bool Contains(string item) { return _data.Contains(item); }
        public void CopyTo(string[] array, int arrayIndex) { }
        public IEnumerator<string> GetEnumerator() { return _data.GetEnumerator(); }
        public int IndexOf(string item) { return _data.IndexOf(item); }
        public void Insert(int index, string item) { _data.Insert(index, item); }
        public bool Remove(string item) { return _data.Remove(item); }
        public void RemoveAt(int index) { _data.RemoveAt(index); }
        IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }
        public void AddRange(string stringToSplit, char[] separator) { }
        public void AddRange(IList collection) { }

        public void CopyTo(IList<string> destination)
        {
            foreach (var r in _data)
            {
                destination.Add(r);
            }
        }

        public void AddRange(IEnumerable<string> collection) { }
    }

    internal class StubMetaRobots : IMetaRobotsValueCollection
    {
        private readonly List<string> _data = new();

        public int IndexOf(string item)
        {
            return _data.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        public string this[int index] { get { return _data[index]; } set { _data[index] = value; } }
        public int Count { get { return _data.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(string item) { _data.Add(item); }
        public void Clear() { _data.Clear(); }
        public bool Contains(string item) { return _data.Contains(item); }
        public void CopyTo(string[] array, int arrayIndex) { }
        public IEnumerator<string> GetEnumerator() { return _data.GetEnumerator(); }
        public bool Remove(string item) { return _data.Remove(item); }
        IEnumerator IEnumerable.GetEnumerator() { return _data.GetEnumerator(); }
        public void AddRange(string stringToSplit, char[] separator) { }
        public void AddRange(IEnumerable<string> collection) { }
        public string GetMetaRobotsContentString() { return string.Empty; }

        public void CopyTo(IList<string> destination)
        {
            _data.ForEach(d => destination.Add(d));
        }

        public bool HasNoIndexAndNoFollow { get { return false; } }
        public void CopyTo(IMetaRobotsValueCollection destination) { }
    }

    #endregion

    #region Supporting Dummies

    internal class DummyAttributeAssemblyProviderFactory : IAttributeAssemblyProviderFactory
    {
        public IAttributeAssemblyProvider Create(IEnumerable<string> includeAssemblies,
            IEnumerable<string> excludeAssemblies)
        {
            return new DummyAttributeAssemblyProvider();
        }
    }

    internal class DummyAttributeAssemblyProvider : IAttributeAssemblyProvider
    {
        public IEnumerable<Assembly> GetAssemblies() { return Array.Empty<Assembly>(); }
    }

    internal class DummyDefinitionProvider : IMvcSiteMapNodeAttributeDefinitionProvider
    {
        public IEnumerable<IMvcSiteMapNodeAttributeDefinition>
            GetMvcSiteMapNodeAttributeDefinitions(IEnumerable<Assembly> assemblies)
        {
            return Array.Empty<IMvcSiteMapNodeAttributeDefinition>();
        }
    }

    #endregion
}
