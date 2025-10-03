using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using MvcSiteMapProvider.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using MvcSiteMapProvider.Web.UrlResolver;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Web;
using MvcSiteMapProvider.Web.Mvc;

namespace MvcSiteMapProvider.Tests.Unit.Core
{
    [TestFixture]
    public class SiteMapNodeTests
    {
        private Mock<ISiteMap> _siteMap;
        private Mock<ISiteMapNodePluginProvider> _pluginProvider;
        private Mock<IMvcContextFactory> _mvcContextFactory;
        private Mock<ISiteMapNodeChildStateFactory> _childStateFactory;
        private Mock<ILocalizationService> _localization;
        private Mock<IUrlPath> _urlPath;
        private Mock<HttpContextBase> _httpContext;
        private Mock<HttpRequestBase> _httpRequest;
        private SiteMapNode _node;

        [SetUp]
        public void SetUp()
        {
            _siteMap = new Mock<ISiteMap>();
            _siteMap.SetupGet(s => s.IsReadOnly).Returns(false);
            _siteMap.SetupGet(s => s.UseTitleIfDescriptionNotProvided).Returns(true);
            _pluginProvider = new Mock<ISiteMapNodePluginProvider>();
            _mvcContextFactory = new Mock<IMvcContextFactory>();
            _childStateFactory = new Mock<ISiteMapNodeChildStateFactory>();
            _localization = new Mock<ILocalizationService>();
            _urlPath = new Mock<IUrlPath>();
            _httpContext = new Mock<HttpContextBase>();
            _httpRequest = new Mock<HttpRequestBase>();
            _httpContext.Setup(c => c.Request).Returns(_httpRequest.Object);
            _mvcContextFactory.Setup(x => x.CreateHttpContext()).Returns(_httpContext.Object);

            // Child collections
            _childStateFactory.Setup(f => f.CreateAttributeDictionary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ISiteMap>(), It.IsAny<ILocalizationService>()))
                .Returns(new Mock<IAttributeDictionary>().Object);
            _childStateFactory.Setup(f => f.CreateRouteValueDictionary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ISiteMap>()))
                .Returns(new TestRouteValueDictionary());
            _childStateFactory.Setup(f => f.CreatePreservedRouteParameterCollection(It.IsAny<ISiteMap>()))
                .Returns(new TestPreservedRouteParameterCollection());
            _childStateFactory.Setup(f => f.CreateRoleCollection(It.IsAny<ISiteMap>()))
                .Returns(new TestRoleCollection());
            _childStateFactory.Setup(f => f.CreateMetaRobotsValueCollection(It.IsAny<ISiteMap>()))
                .Returns(new TestMetaRobotsValueCollection());

            // Strategies
            var urlStrategy = new Mock<ISiteMapNodeUrlResolverStrategy>();
            urlStrategy.Setup(s => s.ResolveUrl(It.IsAny<string>(), It.IsAny<ISiteMapNode>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>() ))
                .Returns<string, ISiteMapNode, string, string, string, IDictionary<string, object>>((p,n,a,c,act,rv)=> "/" + (string.IsNullOrEmpty(c)?"home":c).ToLowerInvariant() + "/" + act.ToLowerInvariant());
            _pluginProvider.SetupGet(p => p.UrlResolverStrategy).Returns(urlStrategy.Object);

            var visStrategy = new Mock<ISiteMapNodeVisibilityProviderStrategy>();
            visStrategy.Setup(v => v.IsVisible(It.IsAny<string>(), It.IsAny<ISiteMapNode>(), It.IsAny<IDictionary<string, object >>()))
                .Returns(true);
            _pluginProvider.SetupGet(p => p.VisibilityProviderStrategy).Returns(visStrategy.Object);

            var dynStrategy = new Mock<IDynamicNodeProviderStrategy>();
            dynStrategy.Setup(d => d.GetProvider(It.IsAny<string>())).Returns((IDynamicNodeProvider)null);
            _pluginProvider.SetupGet(p => p.DynamicNodeProviderStrategy).Returns(dynStrategy.Object);

            _urlPath.Setup(u => u.IsAbsoluteUrl(It.IsAny<string>())).Returns(false);
            _urlPath.Setup(u => u.IsExternalUrl(It.IsAny<string>(), It.IsAny<HttpContextBase>())).Returns(false);
            _urlPath.Setup(u => u.ResolveUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns<string,string,string>((u,p,h)=>u);
            _urlPath.Setup(u => u.GetPublicFacingUrl(It.IsAny<HttpContextBase>())).Returns(new Uri("http://localhost/"));
            _urlPath.Setup(u => u.UrlDecode(It.IsAny<string>())).Returns<string>(s=>s);
            _urlPath.Setup(u => u.ResolveContentUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns<string,string,string>((u,p,h)=>u);

            _node = new SiteMapNode(_siteMap.Object, "k1", false, _pluginProvider.Object, _mvcContextFactory.Object,
                _childStateFactory.Object, _localization.Object, _urlPath.Object);
        }

        [Test]
        public void Url_WhenClickableAndNoResolvedUrl_UsesStrategy()
        {
            _node.Controller = "Home";
            _node.Action = "Index";
            var url = _node.Url; // invoke
            Assert.That(url, Is.EqualTo("/home/index"));
        }

        [Test]
        public void CanonicalKey_ThenCanonicalUrl_SettingSecondThrows()
        {
            _node.CanonicalKey = "other";
            Assert.Throws<ArgumentException>(() => _node.CanonicalUrl = "/explicit" );
        }

        [Test]
        public void CanonicalUrl_ThenCanonicalKey_SettingSecondThrows()
        {
            _node.CanonicalUrl = "/explicit";
            Assert.Throws<ArgumentException>(() => _node.CanonicalKey = "other" );
        }

        [Test]
        public void MatchesRoute_WhenExplicitUrlSet_ReturnsFalse()
        {
            _node.Url = "/static"; // sets unresolved
            var match = _node.MatchesRoute(new Dictionary<string, object>{{"controller","Home"},{"action","Index"}});
            Assert.That(match, Is.False);
        }

        [Test]
        public void MatchesRoute_WhenHostMismatch_ReturnsFalse()
        {
            _node.Controller = "Home";
            _node.Action = "Index";
            _node.HostName = "otherhost"; // simulate mismatch
            _urlPath.Setup(u => u.IsPublicHostName("otherhost", It.IsAny<HttpContextBase>())).Returns(false);
            var match = _node.MatchesRoute(new Dictionary<string, object>{{"controller","Home"},{"action","Index"}});
            Assert.That(match, Is.False);
        }

        [Test]
        public void CopyTo_CopiesWithoutException()
        {
            _node.Controller = "Home";
            _node.Action = "Index";
            _node.Title = "Title";
            _node.Description = "Desc";
            _node.HostName = "host";
            _node.Protocol = "http";
            var target = new Mock<ISiteMapNode>();
            target.SetupAllProperties();
            Assert.DoesNotThrow(()=> _node.CopyTo(target.Object));
        }

        // Simple stub classes for required specialized collections
        private class TestRouteValueDictionary : Dictionary<string,object>, IRouteValueDictionary { public void Add(string k, object v, bool t)=>this[k]=v; public void Add(KeyValuePair<string, object> i, bool t)=>this[i.Key]=i.Value; public void AddRange(IDictionary<string, object> items)=> throw new NotImplementedException(); public void AddRange(IDictionary<string, object> items, bool throwIf)=> throw new NotImplementedException(); public void AddRange(string json)=> throw new NotImplementedException(); public void AddRange(string json,bool throwIf)=> throw new NotImplementedException(); public void AddRange(System.Xml.Linq.XElement xml)=> throw new NotImplementedException(); public void AddRange(System.Xml.Linq.XElement xml,bool throwIf)=> throw new NotImplementedException(); public void AddRange(System.Collections.Specialized.NameValueCollection nvc)=> throw new NotImplementedException(); public void AddRange(System.Collections.Specialized.NameValueCollection nvc,bool throwIf)=> throw new NotImplementedException(); public bool ContainsCustomKeys=> true; public bool MatchesRoute(IEnumerable<string> ap, IDictionary<string, object> rv)=> true; public bool MatchesRoute(IDictionary<string, object> rv)=> true; public void CopyTo(IDictionary<string, object> d){ foreach(var kv in this)d[kv.Key]=kv.Value; } }
        private class TestPreservedRouteParameterCollection : List<string>, IPreservedRouteParameterCollection { public void AddRange(string s,char[] sep){ } public void AddRange(IEnumerable<string> c){ if(c!=null) base.AddRange(c);} public void CopyTo(IList<string> d){ foreach(var v in this) d.Add(v);} }
        private class TestRoleCollection : List<string>, IRoleCollection { public void AddRange(string s,char[] sep){ } public void AddRange(System.Collections.IList list){ } public void AddRange(IEnumerable<string> c){ if(c!=null) base.AddRange(c);} public void CopyTo(IList<string> d){ foreach(var v in this) d.Add(v);} }
        private class TestMetaRobotsValueCollection : List<string>, IMetaRobotsValueCollection { public void AddRange(string s,char[] sep){ } public string GetMetaRobotsContentString(){ return string.Join(",",this);} public bool HasNoIndexAndNoFollow{ get { return false; } } public void CopyTo(IList<string> d){ foreach(var v in this) d.Add(v);} }
    }
}
