using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using System.Web;
using MvcSiteMapProvider.Matching;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Security;

namespace MvcSiteMapProvider.Tests.Unit.Core
{
    [TestFixture]
    public class SiteMapTests
    {
        private Mock<ISiteMapPluginProvider> _pluginProvider;
        private Mock<IMvcContextFactory> _mvcFactory;
        private Mock<ISiteMapChildStateFactory> _childFactory;
        private Mock<IUrlPath> _urlPath;
        private Mock<ISiteMapSettings> _settings;
        private Mock<ISiteMapBuilder> _builder;
        private Mock<IAclModule> _acl;
        private SiteMap _siteMap;

        [SetUp]
        public void SetUp()
        {
            _pluginProvider = new Mock<ISiteMapPluginProvider>();
            _mvcFactory = new Mock<IMvcContextFactory>();
            _childFactory = new Mock<ISiteMapChildStateFactory>();
            _urlPath = new Mock<IUrlPath>();
            _settings = new Mock<ISiteMapSettings>();
            _builder = new Mock<ISiteMapBuilder>();
            _acl = new Mock<IAclModule>();

            _settings.SetupGet(s => s.EnableLocalization).Returns(false);
            _settings.SetupGet(s => s.SecurityTrimmingEnabled).Returns(true);
            _settings.SetupGet(s => s.VisibilityAffectsDescendants).Returns(true);
            _settings.SetupGet(s => s.UseTitleIfDescriptionNotProvided).Returns(true);
            _settings.SetupGet(s => s.SiteMapCacheKey).Returns("cache-key");

            _pluginProvider.SetupGet(p => p.SiteMapBuilder).Returns(_builder.Object);
            _pluginProvider.SetupGet(p => p.AclModule).Returns(_acl.Object);

            _childFactory.Setup(f => f.CreateChildNodeCollectionDictionary()).Returns(new Dictionary<ISiteMapNode, ISiteMapNodeCollection>());
            _childFactory.Setup(f => f.CreateKeyDictionary()).Returns(new Dictionary<string, ISiteMapNode>());
            _childFactory.Setup(f => f.CreateParentNodeDictionary()).Returns(new Dictionary<ISiteMapNode, ISiteMapNode>());
            _childFactory.Setup(f => f.CreateUrlDictionary()).Returns(new Dictionary<IUrlKey, ISiteMapNode>());
            _childFactory.Setup(f => f.CreateSiteMapNodeCollection()).Returns(new TestSiteMapNodeCollection());
            _childFactory.Setup(f => f.CreateLockableSiteMapNodeCollection(It.IsAny<ISiteMap>())).Returns(new TestSiteMapNodeCollection());
            _childFactory.Setup(f => f.CreateReadOnlySiteMapNodeCollection(It.IsAny<ISiteMapNodeCollection>())).Returns<ISiteMapNodeCollection>(c=>c);
            _childFactory.Setup(f => f.CreateEmptyReadOnlySiteMapNodeCollection()).Returns(new TestSiteMapNodeCollection());
            _childFactory.Setup(f => f.CreateUrlKey(It.IsAny<ISiteMapNode>())).Returns<ISiteMapNode>(n=> new TestUrlKey(n.UnresolvedUrl, n.HostName));
            _childFactory.Setup(f => f.CreateUrlKey(It.IsAny<string>(), It.IsAny<string>())).Returns<string,string>((u,h)=> new TestUrlKey(u,h));

            var httpContext = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            httpContext.Setup(c => c.Request).Returns(request.Object);
            _mvcFactory.Setup(f => f.CreateHttpContext()).Returns(httpContext.Object);
            _urlPath.Setup(u => u.GetPublicFacingUrl(It.IsAny<HttpContextBase>())).Returns(new Uri("http://localhost/"));
            _urlPath.Setup(u => u.ResolveUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns<string,string,string>((u,p,h)=>u);

            _siteMap = new SiteMap(_pluginProvider.Object, _mvcFactory.Object, _childFactory.Object, _urlPath.Object, _settings.Object);
        }

        private ISiteMapNode CreateNode(string key, string url, bool clickable = true)
        {
            var node = new Mock<ISiteMapNode>();
            node.SetupGet(n => n.Key).Returns(key);
            node.SetupProperty(n => n.Url, url);
            node.SetupGet(n => n.UnresolvedUrl).Returns(url);
            node.SetupGet(n => n.Clickable).Returns(clickable);
            node.Setup(n => n.IsAccessibleToUser()).Returns(true);
            node.SetupGet(n => n.Title).Returns("Title");
            node.SetupGet(n => n.HttpMethod).Returns("GET");
            node.SetupGet(n => n.Action).Returns("Index");
            node.SetupGet(n => n.Route).Returns(string.Empty);
            node.SetupGet(n => n.RouteValues).Returns(new FakeRouteValues());
            node.SetupGet(n => n.PreservedRouteParameters).Returns(new FakePreserved());
            node.SetupGet(n => n.HostName).Returns(string.Empty);
            node.SetupGet(n => n.CanonicalUrlHostName).Returns(string.Empty);
            node.SetupGet(n => n.ImageUrlHostName).Returns(string.Empty);
            return node.Object;
        }

        [Test]
        public void AddNode_DuplicateKey_Throws()
        {
            var n1 = CreateNode("k","/a");
            var n2 = CreateNode("k","/b");
            _siteMap.AddNode(n1);
            Assert.Throws<InvalidOperationException>(()=> _siteMap.AddNode(n2));
        }

        [Test]
        public void AddNode_DuplicateUrl_Throws()
        {
            var n1 = CreateNode("k1","/a");
            var n2 = CreateNode("k2","/a");
            _siteMap.AddNode(n1);
            Assert.Throws<InvalidOperationException>(()=> _siteMap.AddNode(n2));
        }

        [Test]
        public void RemoveNode_RemovesFromCollections()
        {
            var n1 = CreateNode("k1","/a");
            _siteMap.AddNode(n1);
            _siteMap.RemoveNode(n1);
            Assert.That(_siteMap.FindSiteMapNodeFromKey("k1"), Is.Null);
        }

        [Test]
        public void Clear_RemovesAll()
        {
            _siteMap.AddNode(CreateNode("k1","/a"));
            _siteMap.AddNode(CreateNode("k2","/b"));
            _siteMap.Clear();
            Assert.That(_siteMap.RootNode, Is.Null);
        }

        [Test]
        public void BuildSiteMap_DelegatesToBuilderOnce()
        {
            var root = CreateNode("root","/");
            _builder.Setup(b => b.BuildSiteMap(_siteMap, null)).Returns(root);
            _siteMap.BuildSiteMap();
            Assert.That(_siteMap.RootNode, Is.EqualTo(root));
            _siteMap.BuildSiteMap();
            _builder.Verify(b => b.BuildSiteMap(_siteMap, null), Times.Once);
        }

        private class TestSiteMapNodeCollection : List<ISiteMapNode>, ISiteMapNodeCollection { }
        private class TestUrlKey : IUrlKey { public TestUrlKey(string u,string h){RootRelativeUrl=u;HostName=h;} public string HostName {get;} public string RootRelativeUrl {get;} }
        private class FakeRouteValues : Dictionary<string,object>, IRouteValueDictionary { public void Add(string k, object v, bool t)=> this[k]=v; public void Add(System.Collections.Generic.KeyValuePair<string, object> i, bool t)=> this[i.Key]=i.Value; public void AddRange(IDictionary<string, object> items)=> throw new NotImplementedException(); public void AddRange(IDictionary<string, object> items, bool throwIf)=> throw new NotImplementedException(); public void AddRange(string json)=> throw new NotImplementedException(); public void AddRange(string json,bool throwIf)=> throw new NotImplementedException(); public void AddRange(System.Xml.Linq.XElement xml)=> throw new NotImplementedException(); public void AddRange(System.Xml.Linq.XElement xml,bool throwIf)=> throw new NotImplementedException(); public void AddRange(System.Collections.Specialized.NameValueCollection nvc)=> throw new NotImplementedException(); public void AddRange(System.Collections.Specialized.NameValueCollection nvc,bool throwIf)=> throw new NotImplementedException(); public bool ContainsCustomKeys=> false; public bool MatchesRoute(IEnumerable<string> ap, IDictionary<string, object> rv)=> true; public bool MatchesRoute(IDictionary<string, object> rv)=> true; public void CopyTo(IDictionary<string, object> d){ foreach(var kv in this) d[kv.Key]=kv.Value; } }
        private class FakePreserved : List<string>, IPreservedRouteParameterCollection { public void AddRange(string s,char[] sep){ } public void AddRange(IEnumerable<string> c){ if(c!=null) base.AddRange(c);} public void CopyTo(System.Collections.Generic.IList<string> d){ foreach(var v in this) d.Add(v);} }
    }
}
