using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using MvcSiteMapProvider.Builder;
using MvcSiteMapProvider.Globalization;
using MvcSiteMapProvider.Visitor;

namespace MvcSiteMapProvider.Tests.Unit.Core
{
    [TestFixture]
    public class SiteMapBuilderTests
    {
        private Mock<ISiteMapNodeProvider> _nodeProvider;
        private Mock<ISiteMapNodeVisitor> _visitor;
        private Mock<ISiteMapHierarchyBuilder> _hierarchyBuilder;
        private Mock<ISiteMapNodeHelperFactory> _helperFactory;
        private Mock<ICultureContextFactory> _cultureContextFactory;
        private Mock<ICultureContext> _cultureContext;
        private Mock<ISiteMap> _siteMap;

        [SetUp]
        public void SetUp()
        {
            _nodeProvider = new Mock<ISiteMapNodeProvider>();
            _visitor = new Mock<ISiteMapNodeVisitor>();
            _hierarchyBuilder = new Mock<ISiteMapHierarchyBuilder>();
            _helperFactory = new Mock<ISiteMapNodeHelperFactory>();
            _cultureContextFactory = new Mock<ICultureContextFactory>();
            _cultureContext = new Mock<ICultureContext>();
            _siteMap = new Mock<ISiteMap>();

            _helperFactory.Setup(x => x.Create(It.IsAny<ISiteMap>(), It.IsAny<ICultureContext>()))
                .Returns(Mock.Of<ISiteMapNodeHelper>());
            _cultureContextFactory.Setup(x => x.CreateInvariant()).Returns(_cultureContext.Object);
        }

        private SiteMapBuilder Create()
        {
            return new SiteMapBuilder(
                _nodeProvider.Object,
                _visitor.Object,
                _hierarchyBuilder.Object,
                _helperFactory.Object,
                _cultureContextFactory.Object);
        }

        private ISiteMapNodeToParentRelation Rel(string parentKey, string key)
        {
            var node = new Mock<ISiteMapNode>();
            node.SetupGet(n => n.Key).Returns(key);
            node.SetupGet(n => n.Controller).Returns(string.Empty);
            node.SetupGet(n => n.Action).Returns("Index");
            node.SetupGet(n => n.Area).Returns(string.Empty);
            node.SetupGet(n => n.Url).Returns(string.Empty);
            return new SiteMapNodeToParentRelation(parentKey, node.Object, "test");
        }

        [Test]
        public void BuildSiteMap_WhenSingleRoot_AddsRootAndVisits()
        {
            // arrange
            var relations = new List<ISiteMapNodeToParentRelation>
            {
                Rel(null, "root"),
                Rel("root","child1"),
                Rel("root","child2")
            };
            _nodeProvider.Setup(p => p.GetSiteMapNodes(It.IsAny<ISiteMapNodeHelper>()))
                .Returns(relations);
            _hierarchyBuilder.Setup(h => h.BuildHierarchy(_siteMap.Object, It.IsAny<IEnumerable<ISiteMapNodeToParentRelation>>()))
                .Returns(new List<ISiteMapNodeToParentRelation>());

            var builder = Create();

            // act
            var result = builder.BuildSiteMap(_siteMap.Object, null);

            // assert
            Assert.That(result, Is.Not.Null);
            _siteMap.Verify(s => s.AddNode(result), Times.Once);
            _visitor.Verify(v => v.Execute(result), Times.Once);
        }

        [Test]
        public void BuildSiteMap_WhenMultipleRoots_Throws()
        {
            // arrange
            var relations = new List<ISiteMapNodeToParentRelation>
            {
                Rel(null, "root1"),
                Rel(null, "root2")
            };
            _nodeProvider.Setup(p => p.GetSiteMapNodes(It.IsAny<ISiteMapNodeHelper>()))
                .Returns(relations);
            var builder = Create();

            // act / assert
            Assert.Throws<MvcSiteMapException>(() => builder.BuildSiteMap(_siteMap.Object, null));
        }

        [Test]
        public void BuildSiteMap_WhenNoRoot_Throws()
        {
            // arrange
            var relations = new List<ISiteMapNodeToParentRelation>
            {
                Rel("p","c")
            };
            _nodeProvider.Setup(p => p.GetSiteMapNodes(It.IsAny<ISiteMapNodeHelper>()))
                .Returns(relations);
            var builder = Create();

            // act / assert
            Assert.Throws<MvcSiteMapException>(() => builder.BuildSiteMap(_siteMap.Object, null));
        }

        [Test]
        public void BuildSiteMap_WhenOrphans_Throws()
        {
            // arrange root OK but hierarchy builder returns orphans
            var relations = new List<ISiteMapNodeToParentRelation>
            {
                Rel(null, "root"),
                Rel("root","child")
            };
            _nodeProvider.Setup(p => p.GetSiteMapNodes(It.IsAny<ISiteMapNodeHelper>()))
                .Returns(relations);
            _hierarchyBuilder.Setup(h => h.BuildHierarchy(_siteMap.Object, It.IsAny<IEnumerable<ISiteMapNodeToParentRelation>>()))
                .Returns(new List<ISiteMapNodeToParentRelation>
                {
                    // orphan child referencing non-existent parent key
                    Rel("ghost","orphan")
                });
            var builder = Create();

            // act / assert
            Assert.Throws<MvcSiteMapException>(() => builder.BuildSiteMap(_siteMap.Object, null));
        }
    }
}
