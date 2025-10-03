using Moq;
using MvcSiteMapProvider.Collections.Specialized;
using MvcSiteMapProvider.Web.Html;
using NUnit.Framework;
using System.Web.Mvc;

namespace MvcSiteMapProvider.Tests.Unit.Web.Html
{
    [TestFixture]
    public class SiteMapHelperTest
    {
        #region SetUp / TearDown

        private Mock<IViewDataContainer> iView = null;
        private Mock<ViewContext> viewContext = null;

        [SetUp]
        public void Init()
        {
            iView = new Mock<IViewDataContainer>();
            viewContext = new Mock<ViewContext>();
        }

        [TearDown]
        public void Dispose()
        {
            iView = null;
            viewContext = null;
        }

        #endregion

        #region Tests

        [Test]
        public void BuildModel_Case1_Default_ShouldReturnAllNodesAtRootLevel()
        {
            // @Html.MvcSiteMap().SiteMap()

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase1();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode, 
                startingNodeInChildLevel: true, 
                visibilityAffectsDescendants: true);

            // Assert
            // Flat structure - 3 nodes
            Assert.That("Home", Is.EqualTo(result.Nodes[0].Title));
            Assert.That("About", Is.EqualTo(result.Nodes[1].Title));
            Assert.That("Contact", Is.EqualTo(result.Nodes[2].Title));

            // Check counts
            Assert.That(3, Is.EqualTo(result.Nodes.Count));
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children.Count));
            Assert.That(0, Is.EqualTo(result.Nodes[1].Children.Count));
            Assert.That(0, Is.EqualTo(result.Nodes[2].Children.Count));
        }

        [Test]
        public void BuildModel_Case1_StartingNodeNotInChildLevel_ShouldReturnHierarchicalNodes()
        {
            // @Html.MvcSiteMap().SiteMap(false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase1();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: true);

            // Assert
            // Tree structure - 3 nodes
            Assert.That("Home", Is.EqualTo(result.Nodes[0].Title));
            Assert.That("About", Is.EqualTo(result.Nodes[0].Children[0].Title));
            Assert.That("Contact", Is.EqualTo(result.Nodes[0].Children[1].Title));

            // Check Counts
            Assert.That(1, Is.EqualTo(result.Nodes.Count));
            Assert.That(2, Is.EqualTo(result.Nodes[0].Children.Count));
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[0].Children.Count));
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[1].Children.Count));
        }

        [Test]
        public void BuildModel_Case2_StartingNodeNotInChildLevel_ShouldReturnHierarchicalNodes()
        {
            // @Html.MvcSiteMap().Menu(false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase2();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: true);

            // Assert
            Assert.That("Home", Is.EqualTo(result.Nodes[0].Title));
            Assert.That("About", Is.EqualTo(result.Nodes[0].Children[0].Title));
            Assert.That("About Me", Is.EqualTo(result.Nodes[0].Children[0].Children[0].Title));
            Assert.That("About You", Is.EqualTo(result.Nodes[0].Children[0].Children[1].Title));

            // "Contact" is inaccessible - should be skipped. So should its child node "ContactSomebody".
            Assert.That("Categories", Is.EqualTo(result.Nodes[0].Children[1].Title));

            Assert.That("Cameras", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Title));
            Assert.That("Nikon Coolpix 200", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[0].Title));
            Assert.That("Canon Ixus 300", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[1].Title));

            // "Memory Cards" is not visible. None of its children should be visible.
            Assert.That(1, Is.EqualTo(result.Nodes[0].Children[1].Children.Count));
        }

        [Test]
        public void BuildModel_Case2_StartingNodeNotInChildLevel_VisibilyDoesntAffectDescendants_ShouldReturnHierarchialNodes()
        {
            // @Html.MvcSiteMap().SiteMap(null, MvcSiteMapProvider.SiteMaps.Current.RootNode, false, false)

            // Arrange
            var siteMap = HtmlHelperTestCases.CreateFakeSiteMapCase2();
            var startingNode = siteMap.RootNode;
            HtmlHelper helper = new HtmlHelper(this.viewContext.Object, this.iView.Object);
            MvcSiteMapHtmlHelper helperExtension = new MvcSiteMapHtmlHelper(helper, siteMap, false);

            // Act
            var result = SiteMapHelper.BuildModel(
                helper: helperExtension,
                sourceMetadata: new SourceMetadataDictionary(),
                startingNode: startingNode,
                startingNodeInChildLevel: false,
                visibilityAffectsDescendants: false);

            // Assert
            Assert.That("Home", Is.EqualTo(result.Nodes[0].Title));
            Assert.That("About", Is.EqualTo(result.Nodes[0].Children[0].Title));
            Assert.That("About Me", Is.EqualTo(result.Nodes[0].Children[0].Children[0].Title));
            Assert.That("About You", Is.EqualTo(result.Nodes[0].Children[0].Children[1].Title));

            // "Contact" is inaccessible - should be skipped. So should its child node "ContactSomebody".
            Assert.That("Categories", Is.EqualTo(result.Nodes[0].Children[1].Title));

            Assert.That("Cameras", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Title));
            Assert.That("Nikon Coolpix 200", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[0].Title));
            Assert.That("Canon Ixus 300", Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[1].Title));

            // "Memory Cards" is not visible. However its children should be in its place.
            Assert.That("Kingston 256 GB SD", Is.EqualTo(result.Nodes[0].Children[1].Children[1].Title));
            Assert.That("Sony 256 GB SD", Is.EqualTo(result.Nodes[0].Children[1].Children[2].Title));
            Assert.That("Sony SD Card Reader", Is.EqualTo(result.Nodes[0].Children[1].Children[2].Children[0].Title));

            // Check counts
            Assert.That(1, Is.EqualTo(result.Nodes.Count));
            Assert.That(2, Is.EqualTo(result.Nodes[0].Children.Count)); // Home
            Assert.That(2, Is.EqualTo(result.Nodes[0].Children[0].Children.Count)); // About
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[0].Children[0].Children.Count)); // About Me
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[0].Children[1].Children.Count)); // About You
            Assert.That(3, Is.EqualTo(result.Nodes[0].Children[1].Children.Count)); // Categories
            Assert.That(2, Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children.Count)); // Cameras
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[0].Children.Count)); // Nikon Coolpix 200
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[1].Children[0].Children[1].Children.Count)); // Canon Ixus 300
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[1].Children[1].Children.Count)); // Kingston 256 GB SD
            Assert.That(1, Is.EqualTo(result.Nodes[0].Children[1].Children[2].Children.Count)); // Sony 256 GB SD
            Assert.That(0, Is.EqualTo(result.Nodes[0].Children[1].Children[2].Children[0].Children.Count)); // Sony SD Card Reader
        }



        #endregion
    }
}
