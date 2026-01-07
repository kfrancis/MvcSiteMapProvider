using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;
using Moq;
using MvcSiteMapProvider.Security;
using MvcSiteMapProvider.Web.Mvc;
using MvcSiteMapProvider.Web.Mvc.Filters;
using NUnit.Framework;

namespace MvcSiteMapProvider.Tests.Unit.Security
{
    [TestFixture]
    public class AuthorizeAttributeAclModuleTests
    {
        [SetUp]
        public void SetUp()
        {
            _mvcContextFactory = new Mock<IMvcContextFactory>();
            _controllerDescriptorFactory = new Mock<IControllerDescriptorFactory>();
            _controllerBuilder = new Mock<IControllerBuilder>();
            _filterProvider = new Mock<IGlobalFilterProvider>();

            _siteMap = new Mock<ISiteMap>();
            _node = new Mock<ISiteMapNode>();
            _httpContext = new Mock<HttpContextBase>();
            _httpRequest = new Mock<HttpRequestBase>();
            _httpContextItems = new Dictionary<object, object>();

            // Setup basic HTTP context
            _httpContext.Setup(c => c.Request).Returns(_httpRequest.Object);
            _httpContext.Setup(c => c.Items).Returns(_httpContextItems);
            _httpRequest.Setup(r => r.Url).Returns(new Uri("http://localhost/"));
            _mvcContextFactory.Setup(m => m.CreateHttpContext()).Returns(_httpContext.Object);

            _aclModule = new TestableAuthorizeAttributeAclModule(
                _mvcContextFactory.Object,
                _controllerDescriptorFactory.Object,
                _controllerBuilder.Object,
                _filterProvider.Object
            );
        }

        private Mock<IMvcContextFactory> _mvcContextFactory;
        private Mock<IControllerDescriptorFactory> _controllerDescriptorFactory;
        private Mock<IControllerBuilder> _controllerBuilder;
        private Mock<IGlobalFilterProvider> _filterProvider;
        private Mock<ISiteMap> _siteMap;
        private Mock<ISiteMapNode> _node;
        private Mock<HttpContextBase> _httpContext;
        private Mock<HttpRequestBase> _httpRequest;
        private Dictionary<object, object> _httpContextItems;
        private TestableAuthorizeAttributeAclModule _aclModule;

        [Test]
        public void IsAccessibleToUser_WhenNodeNotClickable_ReturnsTrue()
        {
            // Arrange
            _node.SetupGet(n => n.Clickable).Returns(false);

            // Act
            var result = _aclModule.IsAccessibleToUser(_siteMap.Object, _node.Object);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsAccessibleToUser_WhenNodeHasExternalUrl_ReturnsTrue()
        {
            // Arrange
            _node.SetupGet(n => n.Clickable).Returns(true);
            _node.Setup(n => n.HasExternalUrl(It.IsAny<HttpContextBase>())).Returns(true);

            // Act
            var result = _aclModule.IsAccessibleToUser(_siteMap.Object, _node.Object);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void FindRoutesForNode_WithMvcNode_ConstructsRouteDataDirectly()
        {
            // Arrange
            _node.SetupGet(n => n.Controller).Returns("Test");
            _node.SetupGet(n => n.Action).Returns("Action");
            _node.SetupGet(n => n.Area).Returns("Admin");

            // Act
            var result = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Values["controller"], Is.EqualTo("Test"));
            Assert.That(result?.Values["action"], Is.EqualTo("Action"));
            Assert.That(result?.DataTokens["area"], Is.EqualTo("Admin"));
            
            // Verify that GetRouteData was never called (optimization)
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Never);
        }

        [Test]
        public void FindRoutesForNode_WithMvcNodeNoArea_ConstructsRouteDataWithoutArea()
        {
            // Arrange
            _node.SetupGet(n => n.Controller).Returns("Home");
            _node.SetupGet(n => n.Action).Returns("Index");
            _node.SetupGet(n => n.Area).Returns(string.Empty);

            // Act
            var result = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Values["controller"], Is.EqualTo("Home"));
            Assert.That(result?.Values["action"], Is.EqualTo("Index"));
            Assert.That(result?.DataTokens.ContainsKey("area"), Is.False);
        }

        [Test]
        public void FindRoutesForNode_WithUrlOnlyNode_UsesRoutingFallback()
        {
            // Arrange
            var nodeUrl = "/test/action";
            _node.SetupGet(n => n.Controller).Returns(string.Empty);
            _node.SetupGet(n => n.Action).Returns(string.Empty);
            _node.SetupGet(n => n.Url).Returns(nodeUrl);

            var mockRouteData = new RouteData { Values = { ["controller"] = "Test", ["action"] = "Action" } };

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Returns(mockNodeHttpContext.Object);

            _node.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(mockRouteData);

            // Act
            var result = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Values["controller"], Is.EqualTo("Test"));
        }

        [Test]
        public void FindRoutesForNode_CachesUrlOnlyNodeInHttpContextItems()
        {
            // Arrange
            var nodeUrl = "/test/action";
            _node.SetupGet(n => n.Controller).Returns(string.Empty);
            _node.SetupGet(n => n.Action).Returns(string.Empty);
            _node.SetupGet(n => n.Url).Returns(nodeUrl);

            var mockRouteData = new RouteData { Values = { ["controller"] = "Test", ["action"] = "Action" } };

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Returns(mockNodeHttpContext.Object);

            _node.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(mockRouteData);

            // Act - First call
            var result1 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Act - Second call (should use cache from HttpContext.Items)
            var result2 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert - GetRouteData should only be called once due to per-request caching
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once,
                "GetRouteData should only be called once due to per-request caching");

            Assert.That(result1, Is.Not.Null);
            Assert.That(result2, Is.Not.Null);
            Assert.That(result2?.Values["controller"], Is.EqualTo("Test"));
        }

        [Test]
        public void FindRoutesForNode_CachesNullRouteData()
        {
            // Arrange
            var nodeUrl = "/static/url";
            _node.SetupGet(n => n.Controller).Returns(string.Empty);
            _node.SetupGet(n => n.Action).Returns(string.Empty);
            _node.SetupGet(n => n.Url).Returns(nodeUrl);

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Returns(mockNodeHttpContext.Object);

            _node.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns((RouteData?)null);

            // Act - First call
            var result1 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Act - Second call
            var result2 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert - GetRouteData should only be called once because null is now cached
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once,
                "GetRouteData should be called once because null values are now cached to avoid repeated misses");

            Assert.That(result1, Is.Null);
            Assert.That(result2, Is.Null);
        }

        [Test]
        public void FindRoutesForNode_CacheIsolatedByUrl()
        {
            // Arrange
            var node1 = new Mock<ISiteMapNode>();
            var node2 = new Mock<ISiteMapNode>();

            node1.SetupGet(n => n.Controller).Returns(string.Empty);
            node1.SetupGet(n => n.Action).Returns(string.Empty);
            node1.SetupGet(n => n.Url).Returns("/test/action1");

            node2.SetupGet(n => n.Controller).Returns(string.Empty);
            node2.SetupGet(n => n.Action).Returns(string.Empty);
            node2.SetupGet(n => n.Url).Returns("/test/action2");

            var routeData1 = new RouteData { Values = { ["controller"] = "Test", ["action"] = "Action1" } };
            var routeData2 = new RouteData { Values = { ["controller"] = "Test", ["action"] = "Action2" } };

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Returns(mockNodeHttpContext.Object);

            node1.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(routeData1);
            node2.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(routeData2);

            // Act - Call for both nodes
            var result1 = _aclModule.PublicFindRoutesForNode(node1.Object, _httpContext.Object);
            var result2 = _aclModule.PublicFindRoutesForNode(node2.Object, _httpContext.Object);

            // Assert - Each URL should have its own cache entry
            node1.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);
            node2.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);

            Assert.That(result1?.Values["action"], Is.EqualTo("Action1"));
            Assert.That(result2?.Values["action"], Is.EqualTo("Action2"));
        }

        [Test]
        public void FindRoutesForNode_CachePersistsAcrossMultipleCalls()
        {
            // Arrange
            var nodeUrl = "/test/action";
            _node.SetupGet(n => n.Controller).Returns(string.Empty);
            _node.SetupGet(n => n.Action).Returns(string.Empty);
            _node.SetupGet(n => n.Url).Returns(nodeUrl);

            var mockRouteData = new RouteData { Values = { ["controller"] = "Test", ["action"] = "Action" } };

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Returns(mockNodeHttpContext.Object);

            _node.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(mockRouteData);

            // Act - Multiple calls
            var result1 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);
            var result2 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);
            var result3 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);
            var result4 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert - GetRouteData should only be called once
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once,
                "GetRouteData should only be called once regardless of number of calls");

            Assert.That(result1?.Values["controller"], Is.EqualTo("Test"));
            Assert.That(result2?.Values["controller"], Is.EqualTo("Test"));
            Assert.That(result3?.Values["controller"], Is.EqualTo("Test"));
            Assert.That(result4?.Values["controller"], Is.EqualTo("Test"));
        }

        [Test]
        public void FindRoutesForNode_CreatesCorrectNodeUri()
        {
            // Arrange
            const string NodeUrl = "/test/action";
            _node.SetupGet(n => n.Controller).Returns(string.Empty);
            _node.SetupGet(n => n.Action).Returns(string.Empty);
            _node.SetupGet(n => n.Url).Returns(NodeUrl);

            var mockRouteData = new RouteData();
            Uri? capturedUri = null;

            var mockNodeHttpContext = new Mock<HttpContextBase>();
            _mvcContextFactory.Setup(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()))
                .Callback<ISiteMapNode, Uri, TextWriter>((_, u, _) => capturedUri = u)
                .Returns(mockNodeHttpContext.Object);

            _node.Setup(n => n.GetRouteData(It.IsAny<HttpContextBase>()))
                .Returns(mockRouteData);

            // Act
            _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert
            Assert.That(capturedUri, Is.Not.Null);
            Assert.That(capturedUri?.ToString(), Is.EqualTo("http://localhost/test/action"));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenMvcContextFactoryIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new AuthorizeAttributeAclModule(
                    null,
                    _controllerDescriptorFactory.Object,
                    _controllerBuilder.Object,
                    _filterProvider.Object);
            });
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenControllerDescriptorFactoryIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AuthorizeAttributeAclModule(
                    _mvcContextFactory.Object,
                    null,
                    _controllerBuilder.Object,
                    _filterProvider.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenControllerBuilderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AuthorizeAttributeAclModule(
                    _mvcContextFactory.Object,
                    _controllerDescriptorFactory.Object,
                    null,
                    _filterProvider.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenFilterProviderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _ = new AuthorizeAttributeAclModule(
                    _mvcContextFactory.Object,
                    _controllerDescriptorFactory.Object,
                    _controllerBuilder.Object,
                    null));
        }

        // Testable wrapper that exposes protected methods for testing
        private class TestableAuthorizeAttributeAclModule : AuthorizeAttributeAclModule
        {
            public TestableAuthorizeAttributeAclModule(
                IMvcContextFactory mvcContextFactory,
                IControllerDescriptorFactory controllerDescriptorFactory,
                IControllerBuilder controllerBuilder,
                IGlobalFilterProvider filterProvider)
                : base(mvcContextFactory, controllerDescriptorFactory, controllerBuilder, filterProvider)
            {
            }

            public RouteData? PublicFindRoutesForNode(ISiteMapNode node, HttpContextBase httpContext)
            {
                return FindRoutesForNode(node, httpContext);
            }
        }
    }
}
