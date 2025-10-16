using System;
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

            // Setup basic HTTP context
            _httpContext.Setup(c => c.Request).Returns(_httpRequest.Object);
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
        public void FindRoutesForNode_CachesRouteDataOnFirstCall()
        {
            // Arrange
            var nodeUrl = "/test/action";
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

            // Assert - First call creates HTTP context
            _mvcContextFactory.Verify(m => m.CreateHttpContext(
                It.IsAny<ISiteMapNode>(),
                It.IsAny<Uri>(),
                It.IsAny<TextWriter>()), Times.Once);

            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);
            Assert.That(result1, Is.Not.Null);
            Assert.That(result1?.Values["controller"], Is.EqualTo("Test"));
        }

        [Test]
        public void FindRoutesForNode_ReturnsFromCacheOnSecondCall()
        {
            // Arrange
            var nodeUrl = "/test/action";
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

            // Act - First call (should cache)
            var result1 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Act - Second call (should use cache)
            var result2 = _aclModule.PublicFindRoutesForNode(_node.Object, _httpContext.Object);

            // Assert - CreateHttpContext and GetRouteData should only be called once
            _mvcContextFactory.Verify(m => m.CreateHttpContext(
                    It.IsAny<ISiteMapNode>(),
                    It.IsAny<Uri>(),
                    It.IsAny<TextWriter>()), Times.Once,
                "CreateHttpContext should only be called once due to caching");

            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once,
                "GetRouteData should only be called once due to caching");

            // Both results should be the same cached instance
            Assert.That(result2, Is.SameAs(result1));
            Assert.That(result2?.Values["controller"], Is.EqualTo("Test"));
        }

        [Test]
        public void FindRoutesForNode_CachesIndependentlyForDifferentNodes()
        {
            // Arrange
            var node1 = new Mock<ISiteMapNode>();
            var node2 = new Mock<ISiteMapNode>();

            node1.SetupGet(n => n.Url).Returns("/test/action1");
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

            // Assert - Each node should have its own cache entry
            node1.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);
            node2.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Once);

            Assert.That(result1, Is.Not.SameAs(result2));
            Assert.That(result1?.Values["action"], Is.EqualTo("Action1"));
            Assert.That(result2?.Values["action"], Is.EqualTo("Action2"));
        }

        [Test]
        public void FindRoutesForNode_DoesNotCacheNullRouteData()
        {
            // Arrange
            var nodeUrl = "/static/url";
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

            // Assert - GetRouteData should be called twice (not cached)
            _node.Verify(n => n.GetRouteData(It.IsAny<HttpContextBase>()), Times.Exactly(2),
                "GetRouteData should be called twice because null values are not cached");

            Assert.That(result1, Is.Null);
            Assert.That(result2, Is.Null);
        }

        [Test]
        public void FindRoutesForNode_CachePersistsAcrossMultipleCalls()
        {
            // Arrange
            var nodeUrl = "/test/action";
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

            Assert.That(result1, Is.SameAs(result2));
            Assert.That(result2, Is.SameAs(result3));
            Assert.That(result3, Is.SameAs(result4));
        }

        [Test]
        public void FindRoutesForNode_CreatesCorrectNodeUri()
        {
            // Arrange
            const string NodeUrl = "/test/action";
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
